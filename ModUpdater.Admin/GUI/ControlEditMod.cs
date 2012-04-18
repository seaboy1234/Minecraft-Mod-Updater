using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModUpdater.Admin.GUI
{
    public partial class ControlEditMod : UserControl
    {
        public Mod Mod { get; private set; }
        public ControlEditMod(string title, Mod mod = null)
        {
            InitializeComponent();
            Mod = mod;
            if (mod == null)
                Mod = new Mod();
            lblTitle.Text = title;
        }

        private void ControlEditMod_Load(object sender, EventArgs e)
        {
            txtName.Text = Mod.Name;
            txtAuthor.Text = Mod.Author;
            txtFile.Text = Mod.File;
            foreach (string s in Mod.PostDownloadCLI)
            {
                txtPostDownload.AppendText(s + "\r\n");
            }
            foreach (string s in Mod.WhitelistedUsers)
            {
                txtPostDownload.AppendText(s + "\r\n");
            }
            foreach (string s in Mod.BlacklistedUsers)
            {
                txtPostDownload.AppendText(s + "\r\n");
            }
        }
    }
}
