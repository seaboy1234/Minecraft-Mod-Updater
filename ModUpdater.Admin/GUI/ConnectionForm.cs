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
using ModUpdater.Admin.Items;
using ModUpdater.Utility;
using System.Threading;

namespace ModUpdater.Admin.GUI
{
    internal delegate void VoidInvoke();
    public partial class ConnectionForm : Form
    {
        internal Connection Connection { get; private set; }
        private string failReason;
        public ConnectionForm()
        {
            InitializeComponent();
            DialogResult = System.Windows.Forms.DialogResult.None;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            Properties.Settings.Default.Username = txtUsernm.Text;
            Properties.Settings.Default.Password = txtPasswd.Text;
            Properties.Settings.Default.Server = txtSvr.Text;
            Properties.Settings.Default.Port = int.Parse(txtPort.Text);
            Properties.Settings.Default.Save();
            updaterProgressBar1.EndColor = Color.FromArgb(0, 211, 40);
            updaterProgressBar1.StartColor = Color.FromArgb(0, 211, 40);
            updaterProgressBar1.Value = 0;
            Server s = new Server(txtSvr.Text, txtSvr.Text, int.Parse(txtPort.Text));
            Connection = s.GetConnection();
            Connection.RegisterUser(txtUsernm.Text, txtPasswd.Text);
            TaskManager.AddAsyncTask(delegate
            {
                Connection.ProgressChange += new Connection.ProgressChangeEvent(c_ProgressChange);
                try
                {
                    Connection.Connect();
                }
                catch (LoginFailedException ex)
                {
                    failReason = ex.Message;
                }
            });
        }

        void c_ProgressChange(ProgressStep step)
        {
            if (InvokeRequired)
            {
                Invoke(new Connection.ProgressChangeEvent(c_ProgressChange), step); // Need to call this on the main thread.
                return;
            }
            if (step == ProgressStep.LoginFailed || step == ProgressStep.ConnectionFailed)
            {
                updaterProgressBar1.EndColor = Color.FromArgb(225, 0, 0);
                updaterProgressBar1.StartColor = Color.FromArgb(225, 0, 0);
                lblStatus.Text = string.Format(Connection.ProgressMessage, Connection.FailureMessage);
                btnConnect.Enabled = true;
            }
            else
            {
                updaterProgressBar1.PerformStep();
                lblStatus.Text = Connection.ProgressMessage;
            }
            if (step == ProgressStep.Connected)
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Close();
            }
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            txtUsernm.Text = Properties.Settings.Default.Username;
            txtPasswd.Text = Properties.Settings.Default.Password;
            txtSvr.Text = Properties.Settings.Default.Server;
            txtPort.Text = Properties.Settings.Default.Port.ToString();
        }
        
    }
}
