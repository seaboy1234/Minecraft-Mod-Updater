//    File:        ControlEditMod.cs
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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModUpdater.Admin.Items;

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
                txtWhitelist.AppendText(s + "\r\n");
            }
            foreach (string s in Mod.BlacklistedUsers)
            {
                txtBlacklist.AppendText(s + "\r\n");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
