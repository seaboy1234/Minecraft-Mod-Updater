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
using ModUpdater.Net;
using System.IO;
using ModUpdater.Utility;
using ModUpdater.Admin.Controls;
using System.Threading;

namespace ModUpdater.Admin.GUI
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; }
        internal Connection Connection { get; private set; }
        public string InstancePath { get; private set; }
        private List<Mod> mods, changedMods;
        private int currentDownload, amountOfUpdates, updated, editing;
        private long downloadSize,downloaded;
        private double percentage { get { return ((double)downloaded / downloadSize); } }
        public MainForm()
        {
            InitializeComponent();
            mods = new List<Mod>();
            changedMods = new List<Mod>();
            if (Instance == null) Instance = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1.Controls.Count > 0)
            {
                DialogResult r = MessageBox.Show("Are you sure you want to revert your current changes for the currently selected mod?", Text + " - Alert", MessageBoxButtons.YesNo);
                if (r == System.Windows.Forms.DialogResult.No)
                    return;
                panel1.Controls[0].Dispose();
            }
            editing = -1;
            panel1.Controls.Add(new ControlEditMod("New Mod"));
            ControlEditMod mod = (ControlEditMod)panel1.Controls[0];
            mod.button1.Click += new EventHandler(Mod_button1_Click);
            mod.button2.Click += new EventHandler(Mod_button2_Click);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (panel1.Controls.Count > 0)
            {
                DialogResult r = MessageBox.Show("Are you sure you want to revert your current changes for the currently selected mod?", Text + " - Alert", MessageBoxButtons.YesNo);
                if (r == System.Windows.Forms.DialogResult.No)
                    return;
                panel1.Controls[0].Dispose();
            }
            editing = mods.IndexOf(mods[listBox1.SelectedIndex]);
            panel1.Controls.Add(new ControlEditMod("Edit Mod", mods[listBox1.SelectedIndex]));
            ControlEditMod mod = (ControlEditMod)panel1.Controls[0];
            mod.button1.Click += new EventHandler(Mod_button1_Click);
            mod.button2.Click += new EventHandler(Mod_button2_Click);
        }

        void Mod_button2_Click(object sender, EventArgs e)
        {
            ((UserControl)((Button)sender).Parent).Dispose();
        }

        void Mod_button1_Click(object sender, EventArgs e)
        {
            Mod m = ((ControlEditMod)panel1.Controls[0]).Mod;
            if (editing == -1)
            {
                mods.Add(m);
                listBox1.Items.Add(m);
            }
            else
            {
                mods[editing] = m;
                listBox1.Items[editing] = m;
            }
            button4.Enabled = true;
            changedMods.Add(m);
            panel1.Controls[0].Dispose();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                button2.Enabled = true;
                button3.Enabled = true;
                return;
            }
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ConnectionForm cf = new ConnectionForm();
            if (cf.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                Close();
                return;
            }
            listBox1.Items.AddRange(cf.Connection.Mods);
            mods.AddRange(cf.Connection.Mods);
            Connection = cf.Connection;
            Text = "MCModUpdater AdminCP (" + Connection.Server.Name + ")";
            InstancePath = string.Format("{0}{1}Servers{1}{2}{1}WorkingCopy{1}", Environment.CurrentDirectory, Path.DirectorySeparatorChar, Connection.Server.Name);
            if (!Directory.Exists(InstancePath)) Directory.CreateDirectory(InstancePath);
            int count = 0;
            foreach (Mod m in mods)
            {
                bool exists = File.Exists(InstancePath + m.File);
                if (!exists)
                {
                    m.NeedsUpdate = true;
                    count++;
                    downloadSize += m.Contents.Length;
                    continue;
                }
                bool sameHash = Extras.GenerateHash(InstancePath + m.File) == m.Hash;
                if (!sameHash)
                {
                    m.NeedsUpdate = true;
                    count++;
                    downloadSize += m.Contents.Length;
                    continue;
                }
            }
            if (count > 0)
            {
                amountOfUpdates = count;
                button1.Enabled = false;
                listBox1.Enabled = false;
                panel1.Controls.Add(new ControlDownloadProgress());
                MessageBox.Show(string.Format("{0} mods need to be synced.", count), Text + " - Alert");
                Connection.PacketHandler.RegisterPacketHandler(PacketId.FilePart, HandleFilePart);
                Connection.PacketHandler.RegisterPacketHandler(PacketId.NextDownload, HandleDownloadInfo);
                Connection.PacketHandler.RegisterPacketHandler(PacketId.AllDone, HandleAllDone);
                foreach (Mod m in mods)
                {
                    if (m.NeedsUpdate)
                    {
                        Packet.Send(new RequestModPacket { FileName = m.File, Type = RequestModPacket.RequestType.Download }, Connection.PacketHandler.Stream);
                    }
                    else
                    {
                        m.Contents = File.ReadAllBytes(InstancePath + m.File);
                    }
                }
                TaskManager.AddAsyncTask(HandleSyncScreen);
            }
        }
        private void HandleAllDone(Packet pa)
        {
            AllDonePacket p = pa as AllDonePacket;
            if (!Directory.Exists(Path.GetDirectoryName(InstancePath + p.File))) 
                Directory.CreateDirectory(Path.GetDirectoryName(InstancePath + p.File));
            File.WriteAllBytes(InstancePath + p.File, mods[currentDownload].Contents);
            updated++;
            if (updated == amountOfUpdates)
            {
                Thread.Sleep(3000);
                Invoke(new VoidInvoke(delegate
                {
                    listBox1.Enabled = true;
                }));
            }
        }
        private void HandleDownloadInfo(Packet pa)
        {
            NextDownloadPacket p = pa as NextDownloadPacket;
            currentDownload = mods.IndexOf(mods.Find(new Predicate<Mod>(delegate(Mod m)
            {
                return m.File == p.FileName;
            }))); //I know, get the mod so we can get the index so we can get the mod.
        }
        private void HandleFilePart(Packet pa)
        {
            FilePartPacket p = pa as FilePartPacket;
            int i = p.Index;
            foreach (byte b in p.Part)
            {
                downloaded++;
                mods[currentDownload].Contents[i] = b;
                i++;
            }
        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                button2_Click(null, null);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Mod m = mods[listBox1.SelectedIndex];
            mods.Remove(m);
            listBox1.Items.Remove(m);
            if (changedMods.Contains(m)) changedMods.Remove(m);
            if (File.Exists(InstancePath + Path.DirectorySeparatorChar + m.File)) 
                File.Delete(InstancePath + Path.DirectorySeparatorChar + m.File);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            downloaded = 0; //We're using this as uploaded now.
            downloadSize = 0; //Same here.
            foreach (Mod m in changedMods.ToArray())
            {
                if (m.Hash != Extras.GenerateHash(m.Contents))
                {
                    downloadSize += m.Contents.Length;
                }
            }
            panel1.Controls.Add(new ControlDownloadProgress());
            TaskManager.AddAsyncTask(HandleSyncScreen);
            TaskManager.AddAsyncTask(delegate
            {
                foreach (Mod m in changedMods.ToArray())
                {
                    currentDownload = mods.IndexOf(m); //Again, using this as an upload.
                    Packet.Send(new AdminFileInfoPacket
                    {
                        Author = m.Author,
                        BlacklistedUsers = m.BlacklistedUsers.ToArray(),
                        Description = m.Description,
                        File = m.File,
                        FileSize = m.Size,
                        Hash = m.Hash,
                        ModName = m.Name,
                        PostDownload = m.PostDownloadCLI.ToArray(),
                        WhitelistedUsers = m.WhitelistedUsers.ToArray(),
                        Identifier = m.Identifier
                    }, Connection.PacketHandler.Stream);
                    if (m.Hash != Extras.GenerateHash(m.Contents))
                    {
                        m.Hash = Extras.GenerateHash(m.Contents);
                        List<List<byte>> bytes = new List<List<byte>>();
                        byte[] file = m.Contents;
                        int k = 0;
                        for (int i = 0; i < file.Length; i += 1024)
                        {
                            bytes.Add(new List<byte>());
                            for (int j = i; j < i + 1024; j++)
                            {
                                if (file.Length > j)
                                    bytes[k].Add(file[j]);
                            }
                            k++;
                        }
                        Packet.Send(new UploadFilePacket { Size = m.Size, Parts = bytes.Count, Identifier = m.Identifier }, Connection.PacketHandler.Stream);
                        k = 0;
                        for (int i = 0; i < bytes.Count; i++)
                        {
                            byte[] b = bytes[i].ToArray();
                            Packet.Send(new FilePartPacket { Part = b, Index = k }, Connection.PacketHandler.Stream);
                            k += bytes[i].Count;
                            downloaded = k;
                            Thread.Sleep(25);
                        }
                        Packet.Send(new AllDonePacket { File = m.File }, Connection.PacketHandler.Stream);
                    }
                }
            });
        }
        private void HandleSyncScreen()
        {
            Invoke(new VoidInvoke(delegate
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
            }));
            while (downloaded != downloadSize)
            {
                Invoke(new VoidInvoke(delegate
                {
                    ((ControlDownloadProgress)panel1.Controls[0]).percent.Text = string.Format("{0:0%}", percentage);
                    ((ControlDownloadProgress)panel1.Controls[0]).progress.Value = Convert.ToInt32(percentage.ToString("0%").Replace("%", ""));
                    ((ControlDownloadProgress)panel1.Controls[0]).message.Text = string.Format("{0}", mods[currentDownload].Name);
                }));
                Thread.Sleep(50);
            }
            Thread.Sleep(3000);
            Invoke(new VoidInvoke(delegate
            {
                panel1.Controls[0].Dispose();
                listBox1.SelectedIndex = -1;
                button1.Enabled = true;
                button2.Enabled = false;
                button4.Enabled = false;
            }));
        }
    }
}
