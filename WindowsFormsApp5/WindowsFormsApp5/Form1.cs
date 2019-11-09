using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                await EncryptFile(textBox1.Text);
            else
                await DecryptFile(textBox1.Text);
        }

        private void UpdateProgressBar(int i)
        {
            if (progressBar1.InvokeRequired)
                progressBar1.Invoke(new Action<int>(UpdateProgressBar), i);
            else
            {
                if (progressBar1.Value + i <= 100)
                    progressBar1.Value += i;
            }
        }

        private async Task EncryptFile(string file)
        {
            await Task.Run(async () =>
            {
                string password = textBox2.Text;
                UnicodeEncoding UE = new UnicodeEncoding();
                while (UE.GetBytes(password).Length < 16)
                {
                    password += " ";
                }
                while (UE.GetBytes(password).Length > 16)
                {
                    password = password.Substring(0, 8);
                }
                byte[] key = UE.GetBytes(password);
                byte[] data;
                using (FileStream fsIn = new FileStream(file, FileMode.Open))
                {
                    data = new byte[fsIn.Length];
                    await fsIn.ReadAsync(data, 0, data.Length);
                }
                using (FileStream fsCrypt = new FileStream(file, FileMode.Open))
                {
                    using (CryptoStream cs = new CryptoStream(fsCrypt, new RijndaelManaged().CreateEncryptor(key, key), CryptoStreamMode.Write))
                    {
                        double i = 0;
                        foreach (byte item in data)
                        {
                            cs.WriteByte(item);
                            i += 100.0 / data.Length;
                            if (i >= 1)
                            {
                                UpdateProgressBar((int)i);
                                i = 0;
                            }
                        }
                    }
                }
                UpdateProgressBar(100 - progressBar1.Value);
            });
        }

        private async Task DecryptFile(string file)
        {
            await Task.Run(async () =>
            {
                string password = textBox2.Text;
                UnicodeEncoding UE = new UnicodeEncoding();
                while (UE.GetBytes(password).Length < 16)
                {
                    password += " ";
                }
                while (UE.GetBytes(password).Length > 16)
                {
                    password = password.Substring(0, 8);
                }
                byte[] key = UE.GetBytes(password);
                byte[] data;
                using (FileStream fsCrypt = new FileStream(file, FileMode.Open))
                {
                    using (CryptoStream cs = new CryptoStream(fsCrypt, new RijndaelManaged().CreateDecryptor(key, key), CryptoStreamMode.Read))
                    {
                        data = new byte[fsCrypt.Length];
                        double j = 0;
                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = (byte)cs.ReadByte();
                            j += 100.0 / data.Length;
                            if (j >= 1)
                            {
                                UpdateProgressBar((int)j);
                                j = 0;
                            }
                        }
                    }
                }
                using (FileStream fsOut = new FileStream(file, FileMode.Open))
                {
                    await fsOut.WriteAsync(data, 0, data.Length);
                }
                UpdateProgressBar(100 - progressBar1.Value);
            });
        }
    }
}