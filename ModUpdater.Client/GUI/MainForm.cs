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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using ModUpdater.Utility;
using ModUpdater.Net;
using System.Text;
using ModUpdater.Client.Utility;

namespace ModUpdater.Client.GUI
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; }
        public Mod CurrentDownload;
        public IPAddress LocalAddress;
        public string ServerFolder { get { return serverName.Replace(' ', '_').Replace('.', '-').ToLower(); } }
        public Server Server { get; set; }
        public delegate void Void();
        private PacketHandler ph;
        private Socket socket;
        private List<string> ModFiles = new List<string>();
        private List<Mod> Mods = new List<Mod>();
        private string[] PostDownload;
        private bool ServerShutdown = false;
        private string serverName = "";
        private float serverFontSize = 36;
        private ImageList modImages;
        private bool warnDisconnect = true;
        private int index = 0;
        private int curPart = 0;
        private int Parts = 0;
        private double percentage { get { return ((double)curPart / Parts); } }
        private bool recover;
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
            Program.AppStatus = AppStatus.Updating;
            if (!Properties.Settings.Default.AutoUpdate)
            {
                if (MessageBox.Show("Are you sure you want to update " + lsModsToUpdate.Items.Count + " mods and delete " + lsModsToDelete.Items.Count + " more?", "Confirm Update Action", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
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
            SplashScreen.GetScreen().Invoke(new Void(delegate
            {
                SplashScreen.GetScreen().lblTitle.Font.Dispose();
                SplashScreen.GetScreen().lblTitle.Font = new Font(FontFamily.GenericSansSerif, serverFontSize);
                SplashScreen.GetScreen().lblTitle.Text = serverName;
            }));
            foreach (object o in lsModsToDelete.Items)
            {
                string m = (string)o;
                string path = Properties.Settings.Default.MinecraftPath + "\\" + Path.GetDirectoryName(m) + Path.GetFileName(m).TrimEnd('\\').Replace("clientmods", "mods");
                File.Delete(Properties.Settings.Default.MinecraftPath + @"\mods\" + Path.GetFileName(m));
            }
            Mod mod = (Mod)lsModsToUpdate.Items[index];
            if (lsModsToUpdate.Items.Contains(mod))
            {
                Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, Identifier = mod.File }, ph.Stream);
            }
            TaskManager.AddAsyncTask(delegate
            {
                while (CurrentDownload == null) ;
                while (warnDisconnect == true)
                {
                    SplashScreen.GetScreen().Invoke(new Void(delegate
                    {
                        SplashScreen.GetScreen().lblProgress.Text = string.Format("{0:0%}", percentage);
                        SplashScreen.GetScreen().Progress.Value = Convert.ToInt32(percentage.ToString("0%").Replace("%", ""));
                    }));
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
            PrepareConnection();
            Connect();
        }
        private void PrepareConnection()
        {
            ConnectionForm cf = new ConnectionForm();
            if (cf.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                Close();
                return;
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
                Invoke(new Void(delegate
                {
                    Close();
                }));
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
            Packet.Send(new HandshakePacket { Username = Properties.Settings.Default.Username }, ph.Stream);
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
            curPart++;
            if (ExceptionHandler.ProgramCrashed)
            {
                ph.Stop();
                return;
            }
            while (SplashScreen.GetScreen().Loading) ;
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
            curPart = 0;
            Parts = p.ChunkSize;
            SplashScreen.GetScreen().Invoke(new Void(delegate
            {
                if (!SplashScreen.GetScreen().lblProgress.Visible) SplashScreen.GetScreen().lblProgress.Visible = true;
                SplashScreen.GetScreen().lblProgress.Text = "0%";
            }));
            SplashScreen.GetScreen().Progress.Value = 0;
            SplashScreen.GetScreen().Progress.Step = 10;
            CurrentDownload = Mods.Find(p.Identifier);
            CurrentDownload.Contents = new byte[CurrentDownload.Size];
            if(!ServerShutdown)
                SplashScreen.UpdateStatusText("Downloading " + CurrentDownload.Name);
            else
                SplashScreen.UpdateStatusTextWithStatus("Downloading " + CurrentDownload.Name + "(Server Shutdown Mode)", TypeOfMessage.Warning);
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Starting download of " + CurrentDownload.Name);
            if(modImages.Images.ContainsKey(CurrentDownload.File))
                SplashScreen.GetScreen().setDownloadPicture(modImages.Images[CurrentDownload.File]);
            PostDownload = p.PostDownloadCLI;
            string path = Properties.Settings.Default.MinecraftPath + "\\" + CurrentDownload.File.Replace(CurrentDownload.File.Split('\\').Last(), "").TrimEnd('\\').Replace("clientmods", "mods");
            bool exists = Directory.Exists(path);
            if (!exists) Directory.CreateDirectory(path);
        }

        void ph_AllDone(Packet pa)
        {
            AllDonePacket p = pa as AllDonePacket;
            int i = 0;
            while (curPart != Parts)
            {
                if (i > 10)
                {
                    SplashScreen.UpdateStatusText("There was an error while downloading.  Retrying...");
                    Thread.Sleep(5000);
                    Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, Identifier = p.File }, ph.Stream);
                    return;
                }
                i++;
                Thread.Sleep(1000);
            }
            string path = Path.GetDirectoryName(Properties.Settings.Default.MinecraftPath + "\\" + p.File);
            File.WriteAllBytes(path + "\\" + Path.GetFileName(p.File), CurrentDownload.Contents);
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Downloaded " + path + "\\" + Path.GetFileName(p.File));
            ProcessStartInfo pr = new ProcessStartInfo("cmd");
            pr.CreateNoWindow = true;
            pr.UseShellExecute = false;
            pr.RedirectStandardOutput = true;
            pr.RedirectStandardInput = true;
            Process proc = new Process();
            proc.StartInfo = pr;
            proc.Start();
            foreach (string s in PostDownload)
            {
                try
                {
                    proc.StandardInput.WriteLine(s);
                }
                catch (Exception e) { ExceptionHandler.HandleException(e, this); }
            }
            proc.Kill();
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "[Post Download] " + proc.StandardOutput.ReadToEnd());
            if (GetLastModToUpdate().File == p.File)
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
                    Invoke(new Void(delegate
                    {
                        Program.StartMinecraft();
                    }));
                }
                else
                {
                    SplashScreen.CloseSplashScreen();
                }
                Invoke(new Void(delegate
                {
                    Close();
                }));
                return;
            }
            index++;
            Mod m = (Mod)lsModsToUpdate.Items[index];
            Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Download, Identifier = m.File }, ph.Stream);

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
        void ph_ModList(Packet pa)
        {
            ModListPacket p = pa as ModListPacket;
            ModFiles.AddRange(p.Mods);
            if (p.Mods.Length == 0)
            {
                Invoke(new Void(delegate
                {
                    SplashScreen.CloseSplashScreen();
                    Show();
                }));
                return;
            }
            Invoke(new Void(delegate
            {
                if (Visible) Hide();
            }));
            foreach (string s in p.Mods)
            {
                Packet.Send(new RequestModPacket { Identifier = s, Type = RequestModPacket.RequestType.Info }, ph.Stream);
            }
            bool exists = Directory.Exists(Properties.Settings.Default.MinecraftPath + @"\mods");
            if (!exists) Directory.CreateDirectory(Properties.Settings.Default.MinecraftPath + @"\mods");
            foreach (string s in Directory.GetFiles(Properties.Settings.Default.MinecraftPath + @"\mods"))
            {
                string str = @"mods\" + Path.GetFileName(s);
                bool file = ModFiles.Contains(str) || ModFiles.Contains(str.Replace('\\', '/'));
                if (!file)
                    Invoke(new Void(delegate
                    {
                        lsModsToDelete.Items.Add(Path.GetFileName(s));
                    }));
            }
            SplashScreen.AdvanceProgressBar();
            
        }

        void ph_ModInfo(Packet pa)
        {
            ModInfoPacket p = pa as ModInfoPacket;
            Mod m = new Mod { Author = p.Author, File = p.File, Name = p.ModName, Hash = p.Hash, Size = p.FileSize, Description = p.Description, Identifier = p.Identifier };
            Mods.Add(m);
            string path = Path.GetDirectoryName(Properties.Settings.Default.MinecraftPath + "\\" + p.File);
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
            MinecraftModUpdater.Logger.Log(Logger.Level.Debug, "Info: " + m.Name);
            string str = GetLastModFile();
            if (str == m.File && Properties.Settings.Default.AutoUpdate)
            {
                Invoke(new Void(delegate
                {
                    btnConfirm_Click(null, null);
                }));
            }
            else if (str == m.File)
            {
                Invoke(new Void(delegate
                {
                    SplashScreen.CloseSplashScreen();
                    Show();
                }));
            }
            else if(str != m.File && !Properties.Settings.Default.AutoUpdate)
            {
                Invoke(new Void(delegate
                {
                    if (Visible) Hide();
                }));
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
                ServerShutdown = true;
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
                serverName = p.SData[1];
                serverFontSize = p.FData[0];
                Properties.Settings.Default.MinecraftPath = Environment.CurrentDirectory + "/Minecraft/" + ServerFolder;
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Minecraft path set to: {0}", Properties.Settings.Default.MinecraftPath));
                Server.Name = serverName;
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
                Invoke(new Void(delegate
                {
                    Close();
                }));
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            warnDisconnect = false;
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Stopping logging.");
            try
            {
                if (socket.Connected)
                {
                    Packet.Send(new LogPacket { LogMessages = MinecraftModUpdater.Logger.GetMessages() }, ph.Stream);
                    Packet.Send(new DisconnectPacket(), ph.Stream);
                    ph.Stop();
                }
                string[] file = MinecraftModUpdater.Logger.GetMessages();
                File.WriteAllLines("ModUpdater.log", file);
            }
            catch { }
            foreach (TaskThread t in TaskManager.GetTaskThreads())
            {
                TaskManager.KillTaskThread(t);
            }
            try
            {
                Application.Exit();
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
    }
}
