using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ModUpdater.Client
{
    public partial class UpdateForm : Form
    {
        public UpdateForm()
        {
            InitializeComponent();
            label2.Text = String.Format(label2.Text, MinecraftModUpdater.LongAppName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/seaboy1234/Minecraft-Mod-Updater/downloads");
            Application.Exit();
        }
        public static void Open()
        {
            new UpdateForm().ShowDialog();
        }
    }
}
