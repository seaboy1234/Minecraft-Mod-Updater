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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ModUpdater.Client.Utility;
using ModUpdater.Net;
using ModUpdater.Utility;
using Ionic.Zip;

namespace ModUpdater.Client.GUI
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; }
        public Mod CurrentDownload;
        public IPAddress LocalAddress;
        public string ServerFolder { get { return Server.Name.Replace(' ', '_').Replace('.', '-').ToLower(); } }
        public Server Server { get; set; }

        private PacketHandler ph;
        private Socket socket;
        private List<string> identifiers = new List<string>();
        private List<Mod> Mods = new List<Mod>();
        private List<Mod> OptionalMods = new List<Mod>();
        private ImageList modImages;
        private bool warnDisconnect = true;
        private bool recover;
        int[] progress = new int[5];
        private int dlThisSecond;
        private double percentage { get { return ((double)progress[4] / progress[3]); } }
        //private int index = 0;
        //private int curPart = 0;
        //private int Parts = 0;
        //private int totalDlSize = 0;
        //private int totalDlProgress = 0;
        
        public MainForm(bool recover = false)
        {
            if (Instance == null) Instance = this;
            this.recover = recover;
            InitializeComponent();
        }

        private void Recover()
        {
            switch (Program.AppStatus)
            {
                case AppStatus.Init:
                    PrepareConnection();
                    break;
                case AppStatus.Connecting:
                    PrepareConnection();
                    Connect();
                    break;
                case AppStatus.Updating:
                    PrepareConnection();
                    Connect();
                    Properties.Settings.Default.AutoUpdate = true;
                    btnConfirm_Click(null, null);
                    Properties.Settings.Default.AutoUpdate = false;
                    break;
            }
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
                    if (Properties.Settings.Default.FirstRun || !File.Exists(Properties.Settings.Default.MinecraftPath + "/bin/version"))
                        Program.UpdateMinecraft();
                    SplashScreen.CloseSplashScreen();
                    Hide();
                    Program.StartMinecraft();
                }
                Program.RunOnUIThread(delegate
                {
                    Close();
                });
                return;
            }
            Program.AppStatus = AppStatus.Updating;
            if (!Properties.Settings.Default.AutoUpdate)
            {
                if (MessageBox.Show("Are you sure you want to update " + lsModsToUpdate.Items.Count + " mods and delete " + lsModsToDelete.Items.Count + " more?", "Confirm Update Action", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
            }
            foreach (object m in lsModsToUpdate.Items)
            {
                progress[3] += (int)((Mod)m).Size;
            }
            if (!Properties.Settings.Default.AutoUpdate)
            {
                TaskManager.AddAsyncTask(delegate
                {
                    SplashScreen.ShowSplashScreen();
                });
            }
            while (SplashScreen.GetScreen() == null) ;
            while (SplashScreen.GetScreen().Opacity != 1) ;
            if (Properties.Settings.Default.FirstRun || !File.Exists(Properties.Settings.Default.MinecraftPath + "/bin/version"))
                Program.UpdateMinecraft();
            SplashScreen.UpdateStatusText("Downloading Updates...");
            SplashScreen.GetScreen().Invoke(new ModUpdaterDelegate(delegate
            {
                SplashScreen.GetScreen().lblTitle.Font.Dispose();
                SplashScreen.GetScreen().lblTitle.Font = new Font(FontFamily.GenericSansSerif, Server.FontSize);
                SplashScreen.GetScreen().lblTitle.Text = Server.Name;
                SplashScreen.GetScreen().lblProgress.Visible = true;
                SplashScreen.GetScreen().lblProgress.Text = "0%";
            }));
            foreach (object o in lsModsToDelete.Items)
            {
                string m = (string)o;
                string path = Properties.Settings.Default.MinecraftPath + "\\" + Path.GetDirectoryName(m) + Path.GetFileName(m).TrimEnd('\\').Replace("clientmods", "mods");
                File.Delete(Properties.Settings.Default.MinecraftPath + @"\mods\" + Path.GetFileName(m));
            }
            Mod mod = (Mod)lsModsToUpdate.Items[progress[0]];
            if (lsModsToUpdate.Items.Contains(mod))
            {
                Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, Identifier = mod.Identifier }, ph.Stream);
            }
            TaskManager.AddAsyncTask(delegate
            {
                while (CurrentDownload == null) ;
                int i = 5;
                int kbps = 0;
                while (warnDisconnect == true)
                {
                    SplashScreen.GetScreen().Invoke(new ModUpdaterDelegate(delegate
                    {
                        SplashScreen.GetScreen().lblProgress.Text = string.Format(string.Format("{0:0%}", percentage) + " at {0} KB/s", kbps);
                        SplashScreen.GetScreen().Progress.Value = Convert.ToInt32(percentage.ToString("0%").Replace("%", ""));
                        if (i == 10)
                        {
                            kbps = (dlThisSecond) / 1000;
                            dlThisSecond = 0;
                            i = 0;
                        }
                    }));
                    i++;
                    
                    Thread.Sleep(100);
                }
            });
            Hide();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TaskManager.AddAsyncTask(delegate
            {
                string direction = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        direction = stream.ReadToEnd();
                    }
                }
                int first = direction.IndexOf("Address: ") + 9;
                int last = direction.LastIndexOf("</body>");
                direction = direction.Substring(first, last - first);
                LocalAddress = IPAddress.Parse(direction);
            });
            ExceptionHandler.CloseProgram += new ModUpdaterDelegate(ExceptionHandler_CloseProgram);
            Debug.Assert("Debug mode is enabled.  In-depth messages will be displayed.");
            if (ProgramOptions.Debug)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Warning, "Client is running in debug-mode.");
            }
            if (ProgramOptions.CommandLine)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Warning, "Client is running in commandline.");
            }
            TaskManager.AddAsyncTask(delegate
            {
                string ver;
                bool api;
                if (Extras.CheckForUpdate("client", Program.Version, out ver, out api))
                    UpdateForm.Open(ver, api);
            });
            if (Properties.Settings.Default.FirstRun)
            {
                TaskManager.AddAsyncTask(delegate
                {
                    SplashScreen.ShowSplashScreen();
                });
                OnFirstRun();
            }
            if (recover)
            {
                Recover();
                return;
            }
            if (!PrepareConnection())
            {
                Close();
            }
            Connect();
        }

        void ExceptionHandler_CloseProgram()
        {
            MainForm_FormClosing(null, null);
        }
        private bool PrepareConnection()
        {
            ConnectionForm cf = new ConnectionForm();
            if (cf.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }
            TaskManager.AddAsyncTask(delegate
            {
                SplashScreen.ShowSplashScreen();
            });
            Debug.Assert("Launching Program.");
            SplashScreen.UpdateStatusTextWithStatus("Preparing to connect to the update server...", TypeOfMessage.Warning);
            Thread.Sleep(3000);
            SplashScreen.GetScreen().Progress.Step = 20;
            Program.AppStatus = AppStatus.Connecting;
            SplashScreen.UpdateStatusText("Connecting...");
            SplashScreen.GetScreen().Progress.PerformStep();
            return true;
        }
        private void Connect()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket = s;
            Debug.Assert("Creating Objects.");
            try
            {
                string srv = Server.Address;
                int port = Server.Port;
                if (srv == LocalAddress.ToString()) srv = "127.0.0.1";
                ConnectionHandler.ConnectTo(s, srv, port);
                SplashScreen.GetScreen().Progress.PerformStep();
            }
            catch (SocketException ex)
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
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
            modImages = new ImageList();
            modImages.ImageSize = new Size(230, 180);
            modImages.ColorDepth = ColorDepth.Depth32Bit;
            SplashScreen.GetScreen().Progress.PerformStep();
            TaskManager.AddAsyncTask(delegate
            {
                while (s.Connected) ;
                if (!warnDisconnect) return;
                if (SplashScreen.GetScreen() != null)
                {
                    SplashScreen.UpdateStatusTextWithStatus("Lost connection to server.", TypeOfMessage.Error);
                    Thread.Sleep(5000);
                }
                else MessageBox.Show("Lost connection to server.");
                Program.RunOnUIThread(delegate
                {
                    Close();
                });
            });
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Logging started.");
            ph = new PacketHandler(s);
            ph.Start();
            for (int i = 0; i < 10; i++)
            {
                TaskManager.SpawnTaskThread(ThreadRole.Standard);
            }
            TaskManager.AddAsyncTask(delegate
            {
                ph.RegisterPacketHandler(PacketId.Metadata, ph_Metadata);
                ph.RegisterPacketHandler(PacketId.ModInfo, ph_ModInfo);
                ph.RegisterPacketHandler(PacketId.ModList, ph_ModList);
                ph.RegisterPacketHandler(PacketId.AllDone, ph_AllDone);
                ph.RegisterPacketHandler(PacketId.NextDownload, ph_NextDownload);
                ph.RegisterPacketHandler(PacketId.FilePart, ph_FilePart);
                ph.RegisterPacketHandler(PacketId.Image, ph_Image);
                Debug.Assert("Packet Handlers registered.");
                SplashScreen.GetScreen().Progress.PerformStep();
            });
            if ((new LoginForm()).ShowDialog() != DialogResult.OK)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Error, "Login failed");
                SplashScreen.UpdateStatusTextWithStatus("Your login failed.", TypeOfMessage.Error);
                Thread.Sleep(2000);
                SplashScreen.CloseSplashScreen();
                Thread.Sleep(400);
                Close();
                return;
            }
            Thread.Sleep(1000);
            SplashScreen.UpdateStatusText("Connected to server.  Retreving Mod List.");
            Packet.Send(new HandshakePacket { Username = ProgramOptions.Username }, ph.Stream);
            Debug.Assert("Sent Handshake Packet.");
            Thread.Sleep(100);
            for (int i = 0; i < 5; i++)
            {
                SplashScreen.GetScreen().Progress.Value += 1;
                Thread.Sleep(20);
            }
        }
        private void OnFirstRun()
        {
            Properties.Settings.Default.MinecraftPath = Environment.CurrentDirectory + "/Minecraft";
            while (SplashScreen.GetScreen() == null) ;
            while (SplashScreen.GetScreen().Opacity != 1.0) ;
            SplashScreen.UpdateStatusText("Welcome to " + MinecraftModUpdater.ShortAppName + " Version " + MinecraftModUpdater.Version + ".");
            OptionsForm of = new OptionsForm();
            Thread.Sleep(2000);
            of.ShowDialog();
            SplashScreen.CloseSplashScreen();
        }

        void ph_Image(Packet pa)
        {
            ImagePacket p = pa as ImagePacket;
            Image i = Extras.ImageFromBytes(p.Image);
            if (p.Type == ImagePacket.ImageType.Background)
            {
                SplashScreen.BackgroundImage = i;
                if (SplashScreen.GetScreen() != null)
                {
                    SplashScreen.GetScreen().Image.Image = i;
                }
            }
            else
            {
                modImages.Images.Add(p.ShowOn, i);
            }
        }
        void ph_FilePart(Packet pa)
        {
            FilePartPacket p = pa as FilePartPacket;
            dlThisSecond += p.Part.Length;
            progress[1]++;
            progress[4] += p.Part.Length;
            if (ExceptionHandler.ProgramCrashed)
            {
                ph.Stop();
                return;
            }
            int i = p.Index;
            foreach (byte b in p.Part)
            {
                CurrentDownload.Contents[i] = b;
                i++;
            }
            
        }

        void ph_NextDownload(Packet pa)
        {
            NextDownloadPacket p = pa as NextDownloadPacket;
            Thread.Sleep(100);
            progress[1] = 0;
            progress[2] = p.ChunkSize;
            CurrentDownload = Mods.Find(p.Identifier);
            CurrentDownload.Contents = new byte[CurrentDownload.Size];
            if(!Server.Shutdown)
                SplashScreen.UpdateStatusText("Downloading " + CurrentDownload.Name);
            else
                SplashScreen.UpdateStatusTextWithStatus("Downloading " + CurrentDownload.Name + "(Server Shutdown Mode)", TypeOfMessage.Warning);
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Starting download of " + CurrentDownload.Name);
            if(modImages.Images.ContainsKey(CurrentDownload.File))
                SplashScreen.GetScreen().setDownloadPicture(modImages.Images[CurrentDownload.File]);
             CurrentDownload.PostDownload = p.PostDownloadCLI;
            string path = Properties.Settings.Default.MinecraftPath + "\\" + CurrentDownload.File.Replace(CurrentDownload.File.Split('\\').Last(), "").TrimEnd('\\').Replace("clientmods", "mods");
            bool exists = Directory.Exists(path);
            if (!exists) Directory.CreateDirectory(path);
        }

        void ph_AllDone(Packet pa)
        {
            AllDonePacket p = pa as AllDonePacket;
            int i = 0;
            while (progress[1] != progress[2])
            {
                if (i > 10)
                {
                    SplashScreen.UpdateStatusText("There was an error while downloading.  Retrying...");
                    Thread.Sleep(5000);
                    Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, Identifier = p.Identifier }, ph.Stream);
                    return;
                }
                i++;
                Thread.Sleep(1000);
            }
            Mod m = Mods.Find(p.Identifier);
            //Old Code
            //string path = Path.GetDirectoryName(Properties.Settings.Default.MinecraftPath + "\\" + m.File);
            //File.WriteAllBytes(path + "\\" + Path.GetFileName(m.File), CurrentDownload.Contents);
            //MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Downloaded " + path + "\\" + Path.GetFileName(m.File));
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(m.Contents, 0, m.Contents.Length); //Write file contents to stream.
                using (ZipFile zip1 = ZipFile.Read(ms)) //Only to read it in the next line.
                {
                    foreach (ZipEntry e in zip1)
                    {
                        e.Extract(Properties.Settings.Default.MinecraftPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            ProcessStartInfo pr = new ProcessStartInfo("cmd");
            pr.CreateNoWindow = true;
            pr.UseShellExecute = false;
            pr.RedirectStandardOutput = true;
            pr.RedirectStandardInput = true;
            Process proc = new Process();
            proc.StartInfo = pr;
            proc.Start();
            foreach (string s in m.PostDownload)
            {
                try
                {
                    proc.StandardInput.WriteLine(s);
                }
                catch (Exception e) { ExceptionHandler.HandleException(e, this); }
            }
            proc.Kill();
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "[Post Download] " + proc.StandardOutput.ReadToEnd());
            if (GetLastModToUpdate().File == m.File)
            {
                SplashScreen.UpdateStatusText("All files downloaded!");
                Thread.Sleep(1000);
                warnDisconnect = false;
                Packet.Send(new LogPacket { LogMessages = MinecraftModUpdater.Logger.GetMessages() }, ph.Stream);
                Packet.Send(new DisconnectPacket(), ph.Stream);
                ph.RemovePacketHandler(PacketId.Metadata);
                ph.RemovePacketHandler(PacketId.ModInfo);
                ph.RemovePacketHandler(PacketId.ModList);
                ph.RemovePacketHandler(PacketId.NextDownload);
                ph.RemovePacketHandler(PacketId.FilePart);
                ph.RemovePacketHandler(PacketId.AllDone);
                TaskManager.AddAsyncTask(delegate
                {
                    ph.Stop();
                }, ThreadRole.Delayed, 5000);
                if (Properties.Settings.Default.LaunchAfterUpdate)
                {
                    Program.RunOnUIThread(delegate
                    {
                        Program.StartMinecraft();
                    });
                }
                else
                {
                    SplashScreen.CloseSplashScreen();
                }
                Program.RunOnUIThread(delegate
                {
                    Close();
                });
                return;
            }
            progress[0]++;
            m = (Mod)lsModsToUpdate.Items[progress[0]];
            Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, Identifier = m.Identifier }, ph.Stream);

        }
        Mod GetLastModToUpdate()
        {
            return (Mod)lsModsToUpdate.Items[lsModsToUpdate.Items.Count - 1];
        }
        string GetLastModId()
        {
            return identifiers.LastOrDefault();
        }
        void ph_ModList(Packet pa)
        {
            ModListPacket p = pa as ModListPacket;
            identifiers.AddRange(p.Mods);
            if (p.Mods.Length == 0)
            {
                Program.RunOnUIThread(delegate
                {
                    SplashScreen.CloseSplashScreen();
                    Show();
                });
                return;
            }
            Program.RunOnUIThread(delegate
            {
                if (Visible) Hide();
            });
            foreach (string s in p.Mods)
            {
                Packet.Send(new RequestModPacket { Identifier = s, Type = RequestModPacket.RequestType.Info }, ph.Stream);
            }
            bool exists = Directory.Exists(Properties.Settings.Default.MinecraftPath + @"\mods");
            if (!exists) Directory.CreateDirectory(Properties.Settings.Default.MinecraftPath + @"\mods");
            
            SplashScreen.AdvanceProgressBar();
            
        }

        void ph_ModInfo(Packet pa)
        {
            ModInfoPacket p = pa as ModInfoPacket;
            Mod m = new Mod { Author = p.Author, File = p.File, Name = p.ModName, Hash = p.Hash, Size = p.FileSize, Description = p.Description, Identifier = p.Identifier, Optional = p.Optional, Requires = p.Requires.ToList() };
            if (m.Optional)
            {
                OptionalMods.Add(m);
            }
            else
            {
                Mods.Add(m);
            }
            foreach (string file in p.Files)
            {
                string path = Path.GetDirectoryName(Properties.Settings.Default.MinecraftPath + "\\" + file);
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
                if ((!exists && !m.Optional) || (s != m.Hash && !m.Optional))
                {
                    Program.RunOnUIThread(delegate
                    {
                        lsModsToUpdate.Items.Add(m);
                    });
                    continue;
                }
                else if (!m.Optional)
                {
                    Program.RunOnUIThread(delegate
                    {
                        lsMods.Items.Add(m);
                    });
                }
                if (exists && m.Optional && s == m.Hash)
                {
                    Mods.Add(m);
                    Program.RunOnUIThread(delegate
                    {
                        lsMods.Items.Add(m);
                    });
                }
                else if (exists && m.Optional && s != m.Hash)
                {
                    Mods.Add(m);
                    Program.RunOnUIThread(delegate
                    {
                        lsModsToUpdate.Items.Add(m);
                    });
                }
            }
            MinecraftModUpdater.Logger.Log(Logger.Level.Debug, "Info: " + m.Name);
            string str = GetLastModId();
            if (str == m.Identifier)
            {
                foreach (string str1 in Directory.GetFiles(Properties.Settings.Default.MinecraftPath + @"\mods"))
                {
                    string str2 = @"mods\" + Path.GetFileName(str1);
                    Mod mod =  Mods.FindFromFile(str2);
                    bool file = mod != null;
                    if (!file)
                        Program.RunOnUIThread(delegate
                        {
                            lsModsToDelete.Items.Add(Path.GetFileName(str1));
                        });
                }
            }
            if (str == m.Identifier && Properties.Settings.Default.AutoUpdate)
            {
                Program.RunOnUIThread(delegate
                {
                    btnConfirm_Click(null, null);
                });
            }
            else if (str == m.Identifier)
            {
                List<Mod> allMods = new List<Mod>();
                allMods.AddRange(Mods);
                allMods.AddRange(OptionalMods);
                foreach (Mod mod in Mods)
                {
                    mod.BuildRequiredByList(allMods.ToArray().ToList()); //Just so that we don't modify the mod list.
                }
                foreach (Mod mod in OptionalMods)
                {
                    mod.BuildRequiredByList(allMods.ToArray().ToList()); //Just so that we don't modify the mod list.
                }
                Program.RunOnUIThread(delegate
                {
                    SplashScreen.CloseSplashScreen();
                    Show();
                });
            }
            else if(str != m.Identifier && !Properties.Settings.Default.AutoUpdate)
            {
                Program.RunOnUIThread(delegate
                {
                    if (Visible) Hide();
                });
            }
        }

        void ph_Metadata(Packet pa)
        {
            MetadataPacket p = pa as MetadataPacket;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < p.SData.Length; i++)
            {
                sb.AppendFormat("StringData {0}: {1}\r\n", i, p.SData[i]);
            }
            for (int i = 0; i < p.IData.Length; i++)
            {
                sb.AppendFormat("IntData {0}: {1}\r\n", i, p.IData[i]);
            }
            for (int i = 0; i < p.FData.Length; i++)
            {
                sb.AppendFormat("FloatData {0}: {1}\r\n", i, p.FData[i]);
            }
            Debug.Assert(sb.ToString());
            if (p.SData[0] == "shutdown")
            {
                Server.Shutdown = true;
                if (SplashScreen.GetScreen() != null)
                {
                    SplashScreen.UpdateStatusTextWithStatus(p.SData[1], TypeOfMessage.Error);
                    SplashScreen.GetScreen().Progress.StartColor = Color.FromArgb(210, 202, 0);
                    SplashScreen.GetScreen().Progress.EndColor = Color.FromArgb(210, 202, 0);
                }
                else
                    MessageBox.Show(p.SData[1], "Server Shutdown");
                MinecraftModUpdater.Logger.Log(Logger.Level.Error, "Server Shutdown.  Reason: " + p.SData[1]);
            }
            else if (p.SData[0] == "server_name")
            {
                Server.Name = p.SData[1];
                Server.FontSize = p.FData[0];
                Properties.Settings.Default.MinecraftPath = Environment.CurrentDirectory + "/Minecraft/" + ServerFolder;
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Minecraft path set to: {0}", Properties.Settings.Default.MinecraftPath));
            }
            else if (p.SData[0] == "splash_display")
            {
                SplashScreen.UpdateStatusText(p.SData[1]);
            }
            else if (p.SData[0] == "require_version")
            {
                warnDisconnect = false;
                SplashScreen.UpdateStatusTextWithStatus("This server requires API version " + p.SData[1] + " for you to connect.", TypeOfMessage.Error);
                Thread.Sleep(3000);
                SplashScreen.CloseSplashScreen();
                Thread.Sleep(1000);
                Program.RunOnUIThread(delegate
                {
                    Close();
                });
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            warnDisconnect = false;
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Stopping logging.");
            string[] file = MinecraftModUpdater.Logger.GetMessages();
            File.WriteAllLines("ModUpdater.log", file);
            foreach (TaskThread t in TaskManager.GetTaskThreads())
            {
                TaskManager.KillTaskThread(t);
            }
            try
            {
                if (socket.Connected)
                {
                    Packet.Send(new LogPacket { LogMessages = MinecraftModUpdater.Logger.GetMessages() }, ph.Stream);
                    Packet.Send(new DisconnectPacket(), ph.Stream);
                }
            }
            catch { }
        }

        private void ListBox_DoubleClick_Handler(object sender, EventArgs e)
        {
            try
            {
                new ModInfoForm((Mod)((ListBox)sender).SelectedItem).ShowDialog();
            }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                MinecraftModUpdater.Logger.Log(ex);
            }
        }

        private void btnOptional_Click(object sender, EventArgs e)
        {
            SelectModsForm smf = new SelectModsForm(OptionalMods.ToArray(), Mods.ToArray(true));
            smf.ShowDialog();
            if (smf.DialogResult == DialogResult.None) return;
            foreach (Mod m in smf.SelectedMods)
            {
                if (lsModsToDelete.Items.Contains(Path.GetFileName(m.File)))
                {
                    lsModsToDelete.Items.Remove(Path.GetFileName(m.File));
                    lsMods.Items.Add(m);
                    Mods.Add(m);
                }
                else if (!lsModsToUpdate.Items.Contains(m) && !lsMods.Items.Contains(m))
                {
                    lsModsToUpdate.Items.Add(m);
                    Mods.Add(m);
                }
            }
            foreach (Mod m in smf.UnselectedMods)
            {
                bool delete = false;
                if (lsModsToUpdate.Items.Contains(m))
                {
                    lsModsToUpdate.Items.Remove(m);
                    delete = true;
                }
                else if (lsMods.Items.Contains(m))
                {
                    lsMods.Items.Remove(m);
                    delete = true;
                }
                if (delete)
                {
                    lsModsToDelete.Items.Add(Path.GetFileName(m.File));
                    Mods.Remove(m);
                }
            }
        }
    }
}
