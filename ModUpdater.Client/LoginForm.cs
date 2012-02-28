using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace ModUpdater.Client
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            username.Text = Properties.Settings.Default.Username;
            password.Text = Properties.Settings.Default.Password;            
            rempassword.Checked = Properties.Settings.Default.RememberPassword;
        }

        private void startmc_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Username = username.Text;
            if (rempassword.Checked)
                Properties.Settings.Default.Password = password.Text;
            else
                Properties.Settings.Default.Password = "";
            Properties.Settings.Default.RememberPassword = rempassword.Checked;
            Properties.Settings.Default.Save();
            Hide();
            StartMinecraft();
            Close();
        }

        private void StartMinecraft()
        {
            using (StreamWriter log = File.CreateText("log.txt"))
            {
                if (!File.Exists("Minecraft.exe"))
                {
                    Console.WriteLine("Downloading Minecraft.exe...");
                    new WebClient().DownloadFile("https://s3.amazonaws.com/MinecraftDownload/launcher/Minecraft.exe", "Minecraft.exe");
                }
                Console.WriteLine("Starting Minecraft");
                using (StreamWriter sw = File.AppendText("start.bat"))
                {
                    sw.WriteLine(@"SET APPDATA=%cd%");
                    sw.WriteLine(@"Minecraft.exe {0} {1}", Properties.Settings.Default.Username, Properties.Settings.Default.Password);
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
                ProcessStartInfo info = new ProcessStartInfo("cmd", "/c start.bat");
                info.RedirectStandardOutput = true;
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = info;
                proc.Start();
                log.WriteLine(proc.StandardOutput.ReadToEnd());
                while (File.Exists("Minecraft.exe"))
                {
                    try
                    {
                        File.Delete("Minecraft.exe");
                        break;
                    }
                    catch { }
                    System.Threading.Thread.Sleep(10000);
                }
                while (File.Exists("start.bat"))
                {
                    try
                    {
                        File.Delete("start.bat");
                        break;
                    }
                    catch { }
                    System.Threading.Thread.Sleep(10000);
                }
            }
        }
    }
}
