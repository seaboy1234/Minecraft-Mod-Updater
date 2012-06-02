//    File:        ModInfoForm.cs
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
using ModUpdater.Client.Utility;

namespace ModUpdater.Client.GUI
{
    public partial class ModInfoForm : Form
    {
        Mod mod;
        public ModInfoForm(Mod m)
        {
            mod = m;
            InitializeComponent();
        }

        private void ModInfoForm_Load(object sender, EventArgs e)
        {
            lblName.Text = mod.ModName;
            lblAuthor.Text = mod.Author;
            lblFileN.Text = Path.GetFileName(mod.File);
            txtDesc.Text = mod.Description;
            double size = ConvertToKilobytes(mod.Size);
            string fsize;
            try
            {
                if (size > 1024d)
                {
                    string a, b;
                    size = ConvertToMegabytes(mod.Size);
                    a = size.ToString().Split('.')[0].Trim();
                    b = size.ToString().Split('.')[1].Trim().Remove(2);
                    fsize = string.Format("{0}.{1} MB", a, b);
                }
                else
                {
                    string a, b;
                    a = size.ToString().Split('.')[0].Trim();
                    b = size.ToString().Split('.')[1].Trim().Remove(2);
                    fsize = string.Format("{0}.{1} KB", a, b);
                }
            }
            catch (Exception ex)
            {
                MinecraftModUpdater.Logger.Log(ex);
                fsize = "Error";
            }
            lblFileS.Text = fsize;
        }

        private double ConvertToMegabytes(long p)
        {
            return (p / 1024d) / 1024d;
        }
        private double ConvertToKilobytes(long p)
        {
            return p / 1024d;
        }
    }
}
