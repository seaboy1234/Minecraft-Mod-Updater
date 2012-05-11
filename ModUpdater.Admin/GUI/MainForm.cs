//    File:        MainForm.cs
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

namespace ModUpdater.Admin.GUI
{
    public partial class MainForm : Form
    {
        private List<Mod> mods;
        public MainForm()
        {
            InitializeComponent();
            mods = new List<Mod>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(new ControlEditMod("New Mod"));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(new ControlEditMod("Edit Mod", mods[listBox1.SelectedIndex]));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                button2.Enabled = true;
                return;
            }
            button2.Enabled = false;
        }
    }
}
