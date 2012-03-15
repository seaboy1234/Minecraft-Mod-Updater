﻿//    File:        MainForm.cs
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ModUpdater.Client
{
    public partial class MainForm : Form
    {
        List<string> ModFiles = new List<string>();
        List<Mod> Mods = new List<Mod>();
        ModFile CurrentDownload;
        private PacketHandler ph;
        private PacketHandler ph2;
        private Socket socket;
        public delegate void Void();
        private string[] PostDownload;
        private bool ServerShutdown = false;
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (lsModsToUpdate.Items.Count == 0)
            {
                if (Properties.Settings.Default.LaunchAfterUpdate)
                {
                    SplashScreen.CloseSplashScreen();
                    LoginForm l = new LoginForm();
                    Hide();
                    l.ShowDialog();
                }
                try
                {
                    Invoke(new Void(delegate
                    {
                        Close();
                    }));
                }
                catch { }
                return;
            }
            if (!Properties.Settings.Default.AutoUpdate)
            {
                if (MessageBox.Show("Are you sure you want to update " + lsModsToUpdate.Items.Count + " mods and delete " + lsModsToDelete.Items.Count + " more?", "Confirm Update Action", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
                TaskManager.AddAsyncTask(delegate
                {
                    SplashScreen.ShowSplashScreen();
                    Thread.Sleep(100);
                });
            }
            TaskManager.AddDelayedAsyncTask(delegate
            {
                SplashScreen.UpdateStatusText("Downloading Updates...");
            }, 100);
            TaskManager.AddDelayedAsyncTask(delegate
            {
                foreach (object o in lsModsToDelete.Items)
                {
                    string m = (string)o;
                    string path = Properties.Settings.Default.MinecraftPath + "\\" + Path.GetDirectoryName(m) + Path.GetFileName(m).TrimEnd('\\').Replace("clientmods", "mods");
                    File.Delete(Properties.Settings.Default.MinecraftPath + @"\mods\" + Path.GetFileName(m));
                }
                foreach (Mod m in Mods)
                {
                    if (lsModsToUpdate.Items.Contains(m))
                    {
                        Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, FileName = m.File }, ph.Stream);
                        Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Config, FileName = m.File }, ph.Stream);
                    }
                }
            }, 1000);
            Hide();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TaskManager.AddAsyncTask(delegate { throw new Exception("This is a test"); });
            Debug.Assert("Debug mode is enabled.  In-depth messages will be displayed.");
            if (ProgramOptions.Debug)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Warning, "Client is running in debug-mode.");
            }
            if (ProgramOptions.CommandLine)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Warning, "Client is running in commandline.");
            }
            if ((new ConnectionForm()).ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                Dispose();
                return;
            }
            TaskManager.AddAsyncTask(delegate
            {
                SplashScreen.ShowSplashScreen();
            });
            Debug.Assert("Launching Program.");
            Thread.Sleep(500);
            SplashScreen.UpdateStatusTextWithStatus("Preparing to connect to the update server...", TypeOfMessage.Warning);
            this.Opacity = .0;
            Thread.Sleep(3000);
            SplashScreen.UpdateStatusText("Connecting...");
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket = s;
            Debug.Assert("Creating Opjects.");
            try
            {
                s.Connect(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.Server), 4713));
            }
            catch(Exception ex)
            {
                Debug.Assert(ex);
                MessageBox.Show("There was an error while connecting to the update server.  I will now self destruct.");
                Thread.Sleep(1000);
                SplashScreen.UpdateStatusTextWithStatus("Boom!!!", TypeOfMessage.Error);
                Thread.Sleep(5000);
                SplashScreen.UpdateStatusTextWithStatus("That was a joke, by the way.", TypeOfMessage.Warning);
                Thread.Sleep(1000);
                SplashScreen.CloseSplashScreen();
                Thread.Sleep(3000);
                Close();
                return;
            }
            
            TaskManager.AddAsyncTask(delegate
            {
                while (s.Connected) ; 
                SplashScreen.UpdateStatusTextWithStatus("Lost connection to server.", TypeOfMessage.Error);
                if (Visible) MessageBox.Show("Lost connection to server.");
                else Thread.Sleep(5000);
                Invoke(new Void(delegate
                {
                    Close();
                }));
            });
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Logging started.");
            ph = new PacketHandler(s);
            TaskManager.AddAsyncTask(delegate
            {
                ph.Start();
            });
            TaskManager.AddAsyncTask(delegate
            {
                ph.Metadata += new PacketEvent<MetadataPacket>(ph_Metadata);
                ph.ModInfo += new PacketEvent<ModInfoPacket>(ph_ModInfo);
                ph.ModList += new PacketEvent<ModListPacket>(ph_ModList);
                ph.AllDone += new PacketEvent<AllDonePacket>(ph_AllDone);
                ph.NextDownload += new PacketEvent<NextDownloadPacket>(ph_NextDownload);
                ph.FilePart += new PacketEvent<FilePartPacket>(ph_FilePart);
                ph.ClientUpdate += new PacketEvent<ClientUpdatePacket>(ph_ClientUpdate);
                ph.Connect += new PacketEvent<ConnectPacket>(ph_Connect);
                Debug.Assert("Packet Handlers registered.");
            }); 
            Thread.Sleep(1000);
            SplashScreen.UpdateStatusText("Connected to server.  Retreving Mod List.");
            Packet.Send(new HandshakePacket(), ph.Stream);
            Debug.Assert("Sent Handshake Packet.");
            Thread.Sleep(100);
        }

        void ph_Connect(ConnectPacket p)
        {
        }

        void ph_ClientUpdate(ClientUpdatePacket p)
        {
            SplashScreen.UpdateStatusTextWithStatus("Downloading update of " + p.FileName, TypeOfMessage.Warning);
        }

        void ph_FilePart(FilePartPacket p)
        {
            int i = p.Index;
            foreach (byte b in p.Part)
            {
                CurrentDownload.FileContents[i] = b;
                i++;
            }
            TaskManager.AddAsyncTask(delegate
            {
                SplashScreen.GetScreen().Invoke(new Void(delegate
                {
                    SplashScreen.GetScreen().progressBar1.PerformStep();
                }));
            });
        }

        void ph_NextDownload(NextDownloadPacket p)
        {
            Thread.Sleep(100);
            SplashScreen.GetScreen().Invoke(new Void(delegate
            {
                SplashScreen.GetScreen().progressBar1.Value = 0;
                SplashScreen.GetScreen().progressBar1.Maximum = p.ChunkSize * 10;
                SplashScreen.GetScreen().progressBar1.Style = ProgressBarStyle.Blocks;
                SplashScreen.GetScreen().progressBar1.PerformStep();
            }));
            CurrentDownload = new ModFile(p.ModName, p.FileName, p.Length);
            if(!ServerShutdown)
                SplashScreen.UpdateStatusText("Downloading " + p.ModName);
            else
                SplashScreen.UpdateStatusTextWithStatus("Downloading " + p.ModName + "(Server Shutdown Mode)", TypeOfMessage.Warning);
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Starting download of " + p.ModName);
            PostDownload = p.PostDownloadCLI;
            string path = Properties.Settings.Default.MinecraftPath + "\\" + p.FileName.Replace(p.FileName.Split('\\').Last(), "").TrimEnd('\\').Replace("clientmods", "mods");
            bool exists = Directory.Exists(path);
            if (!exists) Directory.CreateDirectory(path);
        }

        void ph_AllDone(AllDonePacket p)
        {
            TaskManager.AddAsyncTask(delegate
            {
                string path = Properties.Settings.Default.MinecraftPath + "\\" + p.File.Replace(p.File.Split('\\').Last(), "").TrimEnd('\\');
                File.WriteAllBytes(path + "\\" + Path.GetFileName(p.File), CurrentDownload.FileContents);
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Downloaded " + path + "\\" + Path.GetFileName(p.File));
            });
            foreach (string s in PostDownload)
            {
                ProcessStartInfo pr = new ProcessStartInfo("cmd", "/c " + s);
                pr.CreateNoWindow = true;
                pr.RedirectStandardOutput = true;
                Process proc = new Process();
                proc.StartInfo = pr;
                proc.Start();
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, "[Post Download] " + proc.StandardOutput.ReadToEnd());
            }
            if (GetLastModToUpdate().File == p.File)
            {
                Packet.Send(new LogPacket { LogMessages = MinecraftModUpdater.Logger.GetMessages() }, ph.Stream);
                SplashScreen.UpdateStatusText("All files downloaded!");
                Thread.Sleep(1000);
                SplashScreen.CloseSplashScreen();
                if (Properties.Settings.Default.LaunchAfterUpdate)
                {
                    LoginForm l = new LoginForm();
                    Invoke(new Void(delegate
                    {
                        Hide();
                    }));
                    l.ShowDialog();
                }
                Packet.Send(new DisconnectPacket(), ph.Stream);
                ph.Stop();
                ph.Metadata -= new PacketEvent<MetadataPacket>(ph_Metadata);
                ph.ModInfo -= new PacketEvent<ModInfoPacket>(ph_ModInfo);
                ph.ModList -= new PacketEvent<ModListPacket>(ph_ModList);
                ph.AllDone -= new PacketEvent<AllDonePacket>(ph_AllDone);
                ph.NextDownload -= new PacketEvent<NextDownloadPacket>(ph_NextDownload);
                ph.FilePart -= new PacketEvent<FilePartPacket>(ph_FilePart);
                ph.ClientUpdate -= new PacketEvent<ClientUpdatePacket>(ph_ClientUpdate);
                ph.Connect -= new PacketEvent<ConnectPacket>(ph_Connect);
                if (socket.Connected) socket.Disconnect(false);
                Invoke(new Void(delegate
                {
                    Close();
                }));
            }
        }
        Mod GetLastModToUpdate()
        {
            Mod m = (Mod)lsModsToUpdate.Items[0];
            foreach(object o in lsModsToUpdate.Items)
            {
                m = (Mod)o;
            }
            return m;
        }
        string GetLastModFile()
        {
            string s = "";
            foreach (string str in ModFiles)
            {
                s = str;
            }
            return s;
        }
        void ph_ModList(ModListPacket p)
        {
            ModFiles.AddRange(p.Mods);
            foreach (string s in p.Mods)
            {
                Packet.Send(new RequestModPacket { FileName = s, Type = RequestModPacket.RequestType.Info }, ph.Stream);
            }
            bool exists = Directory.Exists(Properties.Settings.Default.MinecraftPath + @"\mods");
            if (!exists) Directory.CreateDirectory(Properties.Settings.Default.MinecraftPath + @"\mods");
            foreach (string s in Directory.GetFiles(Properties.Settings.Default.MinecraftPath + @"\mods"))
            {
                string str = @"mods\" + Path.GetFileName(s);
                bool file = ModFiles.Contains(str);
                if (!file)
                    Invoke(new Void(delegate
                    {
                        lsModsToDelete.Items.Add(Path.GetFileName(s));
                    }));
            }
            if (!Properties.Settings.Default.AutoUpdate)
                Invoke(new Void(delegate
                {
                    Opacity = 1;
                    SplashScreen.CloseSplashScreen();
                    Show();
                }));
        }

        void ph_ModInfo(ModInfoPacket p)
        {
            Mod m = new Mod { Author = p.Author, File = p.File, ModName = p.ModName, Hash = p.Hash };
            Mods.Add(m);
            string path = Properties.Settings.Default.MinecraftPath + "\\" + p.File.Replace(p.File.Split('\\').Last(), "").TrimEnd('\\').Replace("clientmods", "mods");
            string s = "";
            bool exists = File.Exists(path + "\\" + Path.GetFileName(m.File));
            if (exists)
            {
                try
                {
                    s = Extras.GenerateHash(path + "\\" + Path.GetFileName(m.File));
                }
                catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
            }
            if (!exists || s != m.Hash)
            {
                if (m.File.Contains("bin") && s == m.Hash)
                    Invoke(new Void(delegate
                    {
                        lsMods.Items.Add(m);
                    }));
                else
                    Invoke(new Void(delegate
                    {
                        lsModsToUpdate.Items.Add(m);
                    }));
            }
            else
            {
                Invoke(new Void(delegate
                {
                    lsMods.Items.Add(m);
                }));
            }
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Info: " + m.ModName);
            string str = GetLastModFile();
            if (str == m.File && Properties.Settings.Default.AutoUpdate)
            {
                Invoke(new Void(delegate
                {
                    btnConfirm_Click(null, null);
                }));
            }
        }

        void ph_Metadata(MetadataPacket p)
        {
            if (p.Data[0] == "shutdown")
            {
                ServerShutdown = true;
                if (SplashScreen.GetScreen() != null)
                    SplashScreen.UpdateStatusTextWithStatus(p.Data[1], TypeOfMessage.Error);
                else
                    MessageBox.Show(p.Data[1], "Server Shutdown");
                MinecraftModUpdater.Logger.Log(Logger.Level.Error, "Server Shutdown.  Reason: " + p.Data[1]);
            }
        }
        struct Mod
        {
            public string ModName;
            public string Author;
            public string File;
            public string Hash;
            public override string ToString()
            {
                return ModName;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Stopping logging.");
            try
            {
                if (socket.Connected)
                {
                    Packet.Send(new LogPacket { LogMessages = MinecraftModUpdater.Logger.GetMessages() }, ph.Stream);
                    ph.Stop();
                }
                else
                {
                    string[] file = MinecraftModUpdater.Logger.GetMessages();

                    File.WriteAllLines("ModUpdater.log", file);
                }
            }
            catch { }
        }
        
    }
}
