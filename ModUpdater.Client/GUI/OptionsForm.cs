//    File:        OptionsForm.cs
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

namespace ModUpdater.Client.GUI
{
    public partial class OptionsForm : Form
    {
        private static readonly string[] WindowsJavaLocations = new string[] // Sorry, all I know is windows.
        {
            @"C:\Program Files\Java\jre6\bin\java.exe",
            @"C:\Program Files\Java\jre7\bin\java.exe",
            @"C:\Program Files (x86)\Java\jre6\bin\java.exe",
            @"C:\Program Files (x86)\Java\jre7\bin\java.exe"
        };
        private bool SettingsSaved;

        public OptionsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Program.TestJava(txtJava.Text))
            {
                MessageBox.Show("Please select a valid java installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Properties.Settings.Default.LaunchAfterUpdate = chkStartMC.Checked;
            Properties.Settings.Default.AutoUpdate = chkAuUpdate.Checked;
            Properties.Settings.Default.Username = txtUsername.Text;
            Properties.Settings.Default.Password = txtPassword.Text;
            Properties.Settings.Default.RememberPassword = chkRemPass.Checked;
            Properties.Settings.Default.JavaPath = txtJava.Text;
            SettingsSaved = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void javaBtn_Click(object sender, EventArgs e)
        {
            foreach (string s in WindowsJavaLocations)
            {
                if (File.Exists(s))
                {
                    if (MessageBox.Show("Found java in " + s + ".  \n" +
                        "Would you like to use it?",
                        "Java Autodetect",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    {
                        txtJava.Text = s;
                        return;
                    }
                }
            }
            openFileDialog1.Filter = "java.exe | java.exe";
            openFileDialog1.InitialDirectory = "C:\\";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtJava.Text = openFileDialog1.FileName;
            }
            
        }

        private void chkAuUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAuUpdate.Checked && !Properties.Settings.Default.AutoUpdate)
            {
                if (MessageBox.Show("WARNING: Turning on the auto updater will automaticly download new updates.  ONLY use this on servers you FULLY TRUST. \r\nAre you sure you want to enable this feature?", "Mod Updater Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    chkAuUpdate.Checked = false;
                }
            }
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            chkStartMC.Checked = Properties.Settings.Default.LaunchAfterUpdate;
            chkAuUpdate.Checked = Properties.Settings.Default.AutoUpdate;
            txtUsername.Text = Properties.Settings.Default.Username;
            txtPassword.Text = Properties.Settings.Default.Password;
            chkRemPass.Checked = Properties.Settings.Default.RememberPassword;
            txtJava.Text = Properties.Settings.Default.JavaPath;
            if (Properties.Settings.Default.FirstRun)
            {
                button2.Enabled = false;
            }
            BringToFront();
        }

        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!SettingsSaved && Properties.Settings.Default.FirstRun) e.Cancel = true;
        }
    }
}
