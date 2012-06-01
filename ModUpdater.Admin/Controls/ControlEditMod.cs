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
using System.IO;
using ModUpdater.Utility;

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
            modSelector.Title = "Find mod";
            modSelector.InitialDirectory = MainForm.Instance.InstancePath;
            modSelector.Filter = "Zip Files|*.zip|Jar Files|*.jar|All Mod Files|*.zip; *.jar|Sound Files|*.ogg; *.mp3";
            DialogResult r = modSelector.ShowDialog();
            if (r != DialogResult.OK) return;
            string filePath = modSelector.FileName, fileName = Path.GetFileName(modSelector.FileName);
            retry:
            modPlacer.SelectedPath = MainForm.Instance.InstancePath;
            r = modPlacer.ShowDialog();
            if (r != DialogResult.OK) return;
            if (!modPlacer.SelectedPath.ToLower().Contains(MainForm.Instance.InstancePath.ToLower()))
            {
                MessageBox.Show("File must be relocated to one of the folders contained within the current server instance.\r\nYou chose " + modPlacer.SelectedPath);
                goto retry;
            }
            string dir = modPlacer.SelectedPath;
            try
            {
                if (File.Exists(dir + Path.DirectorySeparatorChar + fileName))
                    File.Delete(dir + Path.DirectorySeparatorChar + fileName);
                File.Copy(filePath, dir + Path.DirectorySeparatorChar + fileName);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            Mod.File = ((dir + Path.DirectorySeparatorChar).ToLower() + fileName).Replace(MainForm.Instance.InstancePath.ToLower(), "");
            txtFile.Text = Mod.File; //Just update it.
        }

        private void txtFile_TextChanged(object sender, EventArgs e)
        {
            txtFile.Text = Mod.File;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(MainForm.Instance.InstancePath + Path.DirectorySeparatorChar + Mod.File)) return;
            Mod.Author = txtAuthor.Text;
            Mod.Contents = File.ReadAllBytes(MainForm.Instance.InstancePath + Path.DirectorySeparatorChar + Mod.File);
            Mod.Name = txtName.Text;
            Mod.Size = Mod.Contents.Length;
            Mod.Description = "";
            Mod.BlacklistedUsers.Clear();
            foreach (string s in txtBlacklist.Lines)
            {
                Mod.BlacklistedUsers.Add(s);
            }
            Mod.WhitelistedUsers.Clear();
            foreach (string s in txtWhitelist.Lines)
            {
                Mod.WhitelistedUsers.Add(s);
            }
            Mod.PostDownloadCLI.Clear();
            foreach (string s in txtPostDownload.Lines)
            {
                Mod.PostDownloadCLI.Add(s);
            }
            if (Mod.Identifier == null)
            {
                Mod.Identifier = Extras.GenerateHashFromString(Mod.Name);
            }
        }
    }
}
