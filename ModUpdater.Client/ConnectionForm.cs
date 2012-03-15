//    File:        ConnectionForm.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ModUpdater.Client
{
    public partial class ConnectionForm : Form
    {
        public ConnectionForm()
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            InitializeComponent();
            
        }

        private void btnFindMc_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(".minecraft"))
            {
                if (MessageBox.Show("I've found a valid minecraft folder in this directory.  Would you like me to use \"" + Environment.CurrentDirectory + "\\.minecraft\" as the minecraft directory?", "Make this prossess 10x easyer by pressing \"Yes\"", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    txtMcPath.Text = Environment.CurrentDirectory + "\\.minecraft";
                    return;
                }
                else
                {
                    MessageBox.Show("Fine, have it your way.", "Good Luck...");
                }
            }
            if (mcpathfinder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtMcPath.Text = mcpathfinder.SelectedPath;
            else return;
            if (!mcpathfinder.SelectedPath.Contains(".minecraft")) MessageBox.Show("It seems that you did not select a valid .minecraft folder.  If you are 100% sure you selected a valid minecraft install you can continue, though you may want to check just to be sure.");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtMcPath.Text == "")
            {
                Directory.CreateDirectory(".minecraft");
                btnFindMc_Click(null, null);
            }
            Properties.Settings.Default.MinecraftPath = txtMcPath.Text;
            Properties.Settings.Default.Server = txtServer.Text;
            Properties.Settings.Default.LaunchAfterUpdate = checkBox1.Checked;
            Properties.Settings.Default.AutoUpdate = checkBox2.Checked;
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            txtServer.Text = Properties.Settings.Default.Server;
            txtMcPath.Text = Properties.Settings.Default.MinecraftPath;
            checkBox1.Checked = Properties.Settings.Default.LaunchAfterUpdate;
            checkBox2.Checked = Properties.Settings.Default.AutoUpdate;
            KeyDown += new KeyEventHandler(ConnectionForm_KeyDown);
        }

        private void ConnectionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F12)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();
                txtServer.Text = Properties.Settings.Default.Server;
                txtMcPath.Text = Properties.Settings.Default.MinecraftPath;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked && !Properties.Settings.Default.AutoUpdate)
            {
                if (MessageBox.Show("WARNING: Turning on the auto updater will automaticly download new updates.  ONLY use this on servers you FULLY TRUST. \r\nAre you sure you want to enable this feature?", "Mod Updater Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    checkBox2.Checked = false;
                }
            }
        }
    }
}
