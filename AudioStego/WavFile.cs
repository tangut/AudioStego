using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStego
{
    class WavFile
    {
        public string path;
        //-----WaveHeader-----
        public char[] sGroupID; // RIFF
        public uint dwFileLength; // полная длина файла минус 8, которые заняты RIFFом
        public char[] sRiffType;// всегда WAVE

        //-----WaveFormatChunk-----
        public char[] sFChunkID;         // четыре байта: "fmt "
        public uint dwFChunkSize;        // длина заголовка в байтах
        public ushort wFormatTag;       // 1 (MS PCM)
        public ushort wChannels;        // число каналов
        public uint dwSamplesPerSec;    // частота в ГЦ
        public uint dwAvgBytesPerSec;   // распределение ОЗУ, байт в секунду
        public ushort wBlockAlign;      // размер кадра семпла в байтах
        public ushort wBitsPerSample;    // битов в семпле

        //-----WaveDataChunk-----
        public char[] sDChunkID;     // "data"
        public uint dwDChunkSize;    // длина заголовка в байтах
        public byte dataStartPos;  // поизиция начала даты аудио

        // инициализация wav файла
        public WavFile()
        {
            path = Environment.CurrentDirectory;
            //-----WaveHeader-----
            dwFileLength = 0;
            sGroupID = "RIFF".ToCharArray();
            sRiffType = "WAVE".ToCharArray();

            //-----WaveFormatChunk-----
            sFChunkID = "fmt ".ToCharArray();
            dwFChunkSize = 16;
            wFormatTag = 1;
            wChannels = 2;
            dwSamplesPerSec = 44100;
            wBitsPerSample = 16;
            wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
            dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;

            //-----WaveDataChunk-----
            dataStartPos = 44;
            dwDChunkSize = 0;
            sDChunkID = "data".ToCharArray();
        }

        // метод записи wav файла(просто копирование)
        public void WriteWav(string oldpath, string path)
        {
            FileStream fsr = new FileStream(oldpath, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fsr);
            FileStream fsw = null;
            try
            {
                fsw = new FileStream(path, FileMode.CreateNew);
            }
            catch (IOException)
            {
                fsw = new FileStream(path, FileMode.Truncate);
            }
            BinaryWriter w = new BinaryWriter(fsw);
            int pos = 0, len = (int)r.BaseStream.Length; short temp;
            while (pos < len)
            {
                temp = (short)r.ReadInt16();
                w.Write(temp);
                pos += 2;
            }
            r.Close(); w.Close();
            fsr.Close(); fsw.Close();
        }

        // метод записи нового файла из потока, с записью туда сообщения
        public void WriteFile(string oldpath, string path, string messageStr)
        {
            byte DataPos = this.dataStartPos;
            byte[] source;
            using (BinaryReader b = new BinaryReader(File.Open(oldpath, FileMode.Open)))
            {
                int length = (int)b.BaseStream.Length;
                source = b.ReadBytes(length);
            }
            byte[] bufferMessage = Encoding.UTF8.GetBytes(messageStr);
            int sourceLength = bufferMessage.Length * 4; // длина нашего сообщения
            byte[] sourcelen = BitConverter.GetBytes(sourceLength);
            int offlen = DataPos + 1;
            // шифруем длину нашего сообщения в первые 16 байт дата части wav файла в каждые 2 младших бита
            foreach (byte x in sourcelen)
            {
                int multy = 192;
                for (int i = 6; i >= 0; i = i - 2)
                {
                    int output = (x & multy) >> i;
                    multy = multy / 4;
                    int temp = source[offlen] & 252;
                    source[offlen] = Convert.ToByte(temp | output);
                    offlen++;
                }
            }
  
            int offset = offlen;
            try
            {
                if (source.Length < bufferMessage.Length *4 + DataPos+16)
                {
                    throw new Exception("Длина сообщения больше длины файла");
                }

                // записываем в 2 младших бита байта источника два бита сообщения
                foreach (byte x in bufferMessage)
                {
                    int multiply = 192;
                    for (int i = 6; i >= 0; i = i - 2)
                    {
                        int output = (x & multiply) >> i;
                        multiply = multiply / 4;
                        int temp = source[offset] & 252;
                        source[offset] = Convert.ToByte(temp | output);
                        offset++;
                    }
                }
                using (BinaryWriter b = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    foreach (byte i in source)
                    {
                        b.Write(i);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка: {e.Message}");
            }
        }
        // дешифратор сообщения
        public string EncryptFile(string path)
        {
            byte DataPos = this.dataStartPos;
            byte[] source;
            using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                int length = (int)b.BaseStream.Length;
                source = b.ReadBytes(length);
            }
            byte[] bufferlen = new byte[4];
            int lenstep = 0;
            int offlen = 6;
            // вытаскиваем длинну нашего сообщения
            for (int i = DataPos + 1; i < DataPos + 17; i = i + 4)
            {
                offlen = 6;
                int multy = 192;
                int output = 0;
                for (int k = 0; k < 4; k++)
                {
                    int temp = source[i + k];
                    temp = (temp << offlen) & multy;
                    output = output | temp;
                    multy = multy / 4;
                    offlen = offlen - 2;
                }
                bufferlen[lenstep] = Convert.ToByte(output);
                lenstep++;
            }
            int bufferLength = BitConverter.ToInt32(bufferlen, 0);
            byte[] bufferOutput = new byte[bufferLength / 4];
            //извлекаем сообщение из бинарного потока
            int step = 0;
            int offset;
            for (int i = DataPos + 17; i < DataPos + 17 + bufferLength; i = i + 4)
            {
                offset = 6;
                int multiply = 192;
                int output = 0;
                for (int k = 0; k < 4; k++)
                {
                    int temp = source[i + k];
                    temp = (temp << offset) & multiply;
                    output = output | temp;
                    multiply = multiply / 4;
                    offset = offset - 2;
                }
                bufferOutput[step] = Convert.ToByte(output);
                step++;
            }
            string decodeMessage = Encoding.UTF8.GetString(bufferOutput);
            return decodeMessage;
        }

        public string Analyze(WavFile file2, string path1, string path2)
        {
            byte DataPos1 = this.dataStartPos;
            byte DataPos2 = file2.dataStartPos;
            byte[] source1, source2;
            using (BinaryReader b1 = new BinaryReader(File.Open(path1, FileMode.Open)))
            {
                int length1 = (int)b1.BaseStream.Length;
                source1 = b1.ReadBytes(length1);
            }
            using (BinaryReader b2 = new BinaryReader(File.Open(path2, FileMode.Open)))
            {
                int length2 = (int)b2.BaseStream.Length;
                source2 = b2.ReadBytes(length2);
            }
            int arrlen = source1.Length - (DataPos1+1) ;
            int[] NumberArray = new int[arrlen];
            for (int i = 0; i < arrlen; i++)
            {
                NumberArray[i] = 0;
            }
            string str = "Номера измененных байтов данных в файле:";
            int step = 0;
            for (int i = DataPos2 + 1; i < source2.Length; i++)
            {
                if (source2[i] != source1[i])
                {
                    NumberArray[step] = i;
                    step++;
                }
            }
            for (int i = 0; i < arrlen; i++)
            {
                if(NumberArray[i] != 0)
                {
                    str += "  " + Convert.ToString(NumberArray[i]);
                }
            }
            if (str == "Номера измененных байтов данных в файле:")
                str = "В части аудиоданных файла изменений нет.";
            return str;
        }
    }
}

