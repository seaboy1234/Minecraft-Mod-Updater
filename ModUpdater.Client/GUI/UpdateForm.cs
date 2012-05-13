//    File:        UpdateForm.cs
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
using System.Diagnostics;

namespace ModUpdater.Client.GUI
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
        public static void Open(string ver, bool api)
        {
            UpdateForm t = new UpdateForm();
            if(!api)
                t.label2.Text = string.Format("Version {0} for MCModUpdater is now available.", ver);
            else
                t.label2.Text = string.Format("Version {0} for MCModUpdater API is now available.", ver);
            t.ShowDialog();
        }
    }
}
