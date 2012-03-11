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
            string error;
            if (!VerifyAccount(out error))
            {
                MessageBox.Show("Unable to login to Minecraft.  " + error, "Error");
                DialogResult = System.Windows.Forms.DialogResult.Abort;
            }
            Close();
        }

        private bool VerifyAccount(out string error)
        {
            string postdata = "user=" + Properties.Settings.Default.Username + "&password=" + Properties.Settings.Default.Password + "&version=" + int.MaxValue.ToString();
            byte[] post = Encoding.UTF8.GetBytes(postdata);
            WebRequest r = WebRequest.Create("https://login.minecraft.net");
            r.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)r).UserAgent = "Minecraft Mod Updater Login Manager";
            r.Method = "POST";
            r.ContentType = "application/x-www-form-urlencoded";
            r.ContentLength = post.Length;
            Stream s = r.GetRequestStream();
            s.Write(post, 0, post.Length);
            WebResponse wr = r.GetResponse();
            s = wr.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string responce = sr.ReadToEnd();
            error = responce;
            s.Close();
            sr.Close();
            wr.Close();
            if (!responce.Contains(":")) return false;
            return true;
        }
    }
}
