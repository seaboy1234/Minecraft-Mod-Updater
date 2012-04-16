//    File:        SelectServerDialog.cs
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

namespace ModUpdater.Client.GUI
{
    public partial class SelectServerDialog : Form
    {
        private Server[] Servers;
        public Server SelectedServer;
        public SelectServerDialog(Server[] servers)
        {
            InitializeComponent();
            DialogResult = System.Windows.Forms.DialogResult.None;
            Servers = servers;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            SelectedServer = (Server)serverSelecter.SelectedItem;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void chkRemember_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RememberServer = chkRemember.Checked;
        }

        private void SelectServerDialog_Load(object sender, EventArgs e)
        {
            serverSelecter.Items.AddRange(Servers);
            chkRemember.Checked = Properties.Settings.Default.RememberServer;
        }
    }
}
