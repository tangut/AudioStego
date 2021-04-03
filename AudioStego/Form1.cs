using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioStego
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // при открытии файла сразу будем получать исходные данные о нём
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Wav files (*.WAV)|*.wav";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName.ToString();
                // считываем wav файл, получаем его характеристики, выводим нужные в текстбокс
                WavFile wavFile = new WavFile();
                FileStream fsr = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(fsr);
                wavFile.sGroupID = r.ReadChars(4);
                wavFile.dwFileLength = r.ReadUInt32();
                wavFile.sRiffType = r.ReadChars(4);
                wavFile.sFChunkID = r.ReadChars(4);
                wavFile.dwFChunkSize = r.ReadUInt32();
                wavFile.wFormatTag = r.ReadUInt16();
                wavFile.wChannels = r.ReadUInt16();
                wavFile.dwSamplesPerSec = r.ReadUInt32();
                wavFile.dwAvgBytesPerSec = r.ReadUInt32();
                wavFile.wBlockAlign = r.ReadUInt16();
                wavFile.wBitsPerSample = r.ReadUInt16();
                wavFile.sDChunkID = r.ReadChars(4);
                wavFile.dwDChunkSize = r.ReadUInt32();
                wavFile.dataStartPos = (byte)r.BaseStream.Position;
                r.Close();
                fsr.Close();
                textBox2.Text = Convert.ToString(wavFile.dwSamplesPerSec) + " Гц";
                textBox3.Text = Convert.ToString(wavFile.wChannels);
                textBox4.Text = Convert.ToString(wavFile.wBitsPerSample);
                textBox6.Text = Convert.ToString(wavFile.dwDChunkSize) + " Байт";
                textBox7.Text = Convert.ToString(wavFile.dataStartPos);
                textBox8.Text = Convert.ToString(wavFile.dwFChunkSize);
                textBox9.Text = Convert.ToString(wavFile.wBlockAlign);
                textBox10.Text = Convert.ToString(wavFile.dwAvgBytesPerSec);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WavFile wavFile = new WavFile();
            FileStream fsr = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fsr);
            wavFile.sGroupID = r.ReadChars(4);
            wavFile.dwFileLength = r.ReadUInt32();
            wavFile.sRiffType = r.ReadChars(4);
            wavFile.sFChunkID = r.ReadChars(4);
            wavFile.dwFChunkSize = r.ReadUInt32();
            wavFile.wFormatTag = r.ReadUInt16();
            wavFile.wChannels = r.ReadUInt16();
            wavFile.dwSamplesPerSec = r.ReadUInt32();
            wavFile.dwAvgBytesPerSec = r.ReadUInt32();
            wavFile.wBlockAlign = r.ReadUInt16();
            wavFile.wBitsPerSample = r.ReadUInt16();
            wavFile.sDChunkID = r.ReadChars(4);
            wavFile.dwDChunkSize = r.ReadUInt32();
            wavFile.dataStartPos = (byte)r.BaseStream.Position;
            r.Close();
            fsr.Close();
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Wav files (*.WAV)|*.wav";
            int bytesPerSample = Convert.ToInt32(textBox4.Text) / 8;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = saveFile.FileName.ToString();
                //wavFile.WriteWav(textBox1.Text, textBox5.Text);
                wavFile.WriteFile(textBox1.Text, textBox5.Text, richTextBox1.Text);
            }

        }
        // реализация кнопки, отвечающей за дешифровку сообщения из зашифрованного файла
        private void button3_Click(object sender, EventArgs e)
        {
            WavFile wavFile = new WavFile();
            FileStream fsr = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fsr);
            wavFile.sGroupID = r.ReadChars(4);
            wavFile.dwFileLength = r.ReadUInt32();
            wavFile.sRiffType = r.ReadChars(4);
            wavFile.sFChunkID = r.ReadChars(4);
            wavFile.dwFChunkSize = r.ReadUInt32();
            wavFile.wFormatTag = r.ReadUInt16();
            wavFile.wChannels = r.ReadUInt16();
            wavFile.dwSamplesPerSec = r.ReadUInt32();
            wavFile.dwAvgBytesPerSec = r.ReadUInt32();
            wavFile.wBlockAlign = r.ReadUInt16();
            wavFile.wBitsPerSample = r.ReadUInt16();
            wavFile.sDChunkID = r.ReadChars(4);
            wavFile.dwDChunkSize = r.ReadUInt32();
            wavFile.dataStartPos = (byte)r.BaseStream.Position;
            r.Close();
            fsr.Close();
            string decodedMessage = wavFile.EncryptFile(textBox1.Text);
            richTextBox2.Text = decodedMessage;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Оригинальный wav файл (*.WAV)|*.wav";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox11.Text = dialog.FileName.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Измененный wav файл (*.WAV)|*.wav";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox12.Text = dialog.FileName.ToString();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
                WavFile wavFile1 = new WavFile();
                FileStream fsr = new FileStream(textBox11.Text, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(fsr);
                wavFile1.sGroupID = r.ReadChars(4);
                wavFile1.dwFileLength = r.ReadUInt32();
                wavFile1.sRiffType = r.ReadChars(4);
                wavFile1.sFChunkID = r.ReadChars(4);
                wavFile1.dwFChunkSize = r.ReadUInt32();
                wavFile1.wFormatTag = r.ReadUInt16();
                wavFile1.wChannels = r.ReadUInt16();
                wavFile1.dwSamplesPerSec = r.ReadUInt32();
                wavFile1.dwAvgBytesPerSec = r.ReadUInt32();
                wavFile1.wBlockAlign = r.ReadUInt16();
                wavFile1.wBitsPerSample = r.ReadUInt16();
                wavFile1.sDChunkID = r.ReadChars(4);
                wavFile1.dwDChunkSize = r.ReadUInt32();
                wavFile1.dataStartPos = (byte)r.BaseStream.Position;
                r.Close();
                fsr.Close();
                WavFile wavFile2 = new WavFile();
                FileStream fsr1 = new FileStream(textBox12.Text, FileMode.Open, FileAccess.Read);
                BinaryReader r1 = new BinaryReader(fsr1);
                wavFile2.sGroupID = r1.ReadChars(4);
                wavFile2.dwFileLength = r1.ReadUInt32();
                wavFile2.sRiffType = r1.ReadChars(4);
                wavFile2.sFChunkID = r1.ReadChars(4);
                wavFile2.dwFChunkSize = r1.ReadUInt32();
                wavFile2.wFormatTag = r1.ReadUInt16();
                wavFile2.wChannels = r1.ReadUInt16();
                wavFile2.dwSamplesPerSec = r1.ReadUInt32();
                wavFile2.dwAvgBytesPerSec = r1.ReadUInt32();
                wavFile2.wBlockAlign = r1.ReadUInt16();
                wavFile2.wBitsPerSample = r1.ReadUInt16();
                wavFile2.sDChunkID = r1.ReadChars(4);
                wavFile2.dwDChunkSize = r1.ReadUInt32();
                wavFile2.dataStartPos = (byte)r1.BaseStream.Position;
                r1.Close();
                fsr1.Close();
                richTextBox3.Text = wavFile1.Analyze(wavFile2, textBox11.Text, textBox12.Text);
        }
    }
}
