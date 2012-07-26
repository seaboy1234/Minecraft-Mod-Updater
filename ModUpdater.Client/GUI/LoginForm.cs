//    File:        LoginForm.cs
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
using System.Diagnostics;
using System.Net;
using ModUpdater.Utility;

namespace ModUpdater.Client.GUI
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
                return;
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private bool VerifyAccount(out string error)
        {
            LoginManager m = new LoginManager(username.Text);
            string[] returndata;
            error = "";
            bool failed = !m.Login(password.Text, out returndata);
            if (failed)
            {
                error = returndata[0];
                return false;
            }
            ProgramOptions.Username = m.Username;
            ProgramOptions.SessionID = m.SessionID;
            ProgramOptions.LatestVersion = returndata[0];
            return true;
        }
    }
}
