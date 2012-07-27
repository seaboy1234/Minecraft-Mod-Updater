//    File:        Server.cs
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
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Drawing;
using ModUpdater.Net;
using ModUpdater.Utility;

namespace ModUpdater.Server
{
    class Server
    {
        public List<Mod> Mods { get; private set; }
        public List<Client> Clients { get; private set; }
        public TcpListener TcpServer { get; private set; }
        public TcpListener TcpServer2 { get; private set; }
        public IPAddress Address { get; private set; }
        public Dictionary<Mod, Image> ModImages { get; private set; }
        public Image BackgroundImage { get; private set; }
        public List<string> Administrators { get; private set; }
        private StreamWriter logFile;
        bool Online { get; set; }
        public Server()
        {
            MinecraftModUpdater.Logger.LogEvent += new LogEventDelegate(Logger_LogEvent);
            System.Diagnostics.Process.GetCurrentProcess().Exited += new EventHandler(Server_Exited);
            Config.Load();
            logFile = File.AppendText(Config.LogFile);
            logFile.AutoFlush = true;
            Mods = new List<Mod>();
            Clients = new List<Client>();
            TcpServer = new TcpListener(IPAddress.Any, Config.Port);
            ModImages = new Dictionary<Mod, Image>();
            Administrators = new List<string>();
            MCModUpdaterExceptionHandler.RegisterExceptionHandler(new ExceptionHandler());
            SelfUpdate();
            Administrators.AddRange(File.ReadAllLines("administrators.txt"));
            LoadMods();
            if (File.Exists(Config.ModsPath + "/assets/server_background.png"))
            {
                BackgroundImage = Image.FromFile(Config.ModsPath + "/assets/server_background.png");
            }
        }

        void Server_Exited(object sender, EventArgs e)
        {
            Dispose();
        }
        
        private void SelfUpdate()
        {
            //Update
            switch (Config.Version)
            {
                case "1.2.x":
                    Upgrade.From12x();
                    break;

            }
            //Install
            if (!File.Exists("administrators.txt"))
            {
                Stream f = File.Create("administrators.txt");
                f.Flush();
                f.Close();
            }
            if (!Directory.Exists(Config.ModsPath + "/mods")) Directory.CreateDirectory(Config.ModsPath + "/mods");
            if (!Directory.Exists(Config.ModsPath + "/xml")) Directory.CreateDirectory(Config.ModsPath + "/xml");
            if (!Directory.Exists(Config.ModsPath + "/assets")) Directory.CreateDirectory(Config.ModsPath + "/assets");
            if (!Directory.Exists(Config.ModsPath + "/assets/mod")) Directory.CreateDirectory(Config.ModsPath + "/assets/mod");
        }
        private void LoadMods()
        {
            foreach (string s in Directory.GetFiles(Config.ModsPath + "/xml"))
            {
                try
                {
                    Mods.Add(new Mod(s));
                }
                catch { } //This error is handled at a lower level.
            }
            foreach (Mod m in Mods)
            {
                if (File.Exists(Config.ModsPath + "/ModAssets/" + Path.GetFileName(m.ModFile) + ".png"))
                {
                    ModImages.Add(m, Image.FromFile(Config.ModsPath + "/ModAssets/" + Path.GetFileName(m.ModFile) + ".png"));
                }
                m.LoadRequiredMods(Mods);
            }
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Registered {0} mods", Mods.Count);
        }
        public void Start()
        {
            Address = IPAddress.Loopback;
            try
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
                Address = IPAddress.Parse(direction);
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
            MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Server IP Address is: " + Address.ToString());
            TcpServer.Start();
            Online = true;
            TaskManager.AddAsyncTask(delegate { SimpleConsoleInputHandler(); });
            TaskManager.AddAsyncTask(delegate
            {
                if (Config.MasterServer != "")
                {
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    string ip = Config.MasterServer.Split(':')[0].Trim();
                    int port = int.Parse(Config.MasterServer.Split(':')[1].Trim());
                    ConnectionHandler.ConnectTo(s, ip, port);
                    PacketHandler ph = new PacketHandler(s);
                    ph.Start();
                    Thread.Sleep(1000);
                    Packet.Send(new HandshakePacket { Name = Config.ServerName, Port = Config.Port, Address = Address.ToString(), Type = HandshakePacket.SessionType.Server }, ph.Stream);
                }
            }, ThreadRole.Delayed, 1000);
            TaskManager.AddAsyncTask(delegate
            {
                string ver;
                bool api;
                if (Extras.CheckForUpdate("server", Program.Version, out ver, out api))
                {
                    if (!api)
                        MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Version {0} is now available for Minecraft Mod Updater.", ver);
                    else
                        MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Version {0} is now available for Minecraft Mod Updater API.", ver);
                }
            });
            Receive();
        }
        public void Dispose()
        {
            TcpServer.Stop();
            Online = false;
            foreach (Mod m in Mods)
            {
                m.Save();
            }
            Config.Save();
            logFile.Flush();
            logFile.Close();
            Thread.Sleep(300);
        }
        public void Receive()
        {
            while (Online)
            {
                try
                {
                    Socket s = TcpServer.AcceptSocket();
                    TaskManager.SpawnTaskThread(ThreadRole.Standard);
                    TaskManager.AddAsyncTask(delegate
                    {
                        AcceptClient(s);
                    });
                }
                catch { }
            }
        }
        public void AcceptClient(Socket s)
        {
            try
            {
                if (Clients.Count >= Config.MaxClients)
                {
                    s.Disconnect(false);
                    return;
                }
                Client c = new Client(s, this);
                c.ClientDisconnected += delegate
                {
                    Clients.Remove(c);
                    TaskManager.KillTaskThread(TaskManager.GetTaskThread(Thread.CurrentThread));
                };
                Clients.Add(c);
                c.StartListening();
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
        }
        void Logger_LogEvent(Logger.Level level, string message)
        {
            if (level > Logger.Level.Debug)
            {
                Console.WriteLine("{0} [{1}] {2}", DateTime.Now, level.ToString().ToUpper(), message);
                logFile.WriteLine("{0} [{1}] {2}", DateTime.Now, level.ToString().ToUpper(), message);
            }
        }
        public void SimpleConsoleInputHandler()
        {
            MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Simple Console Input Handler is online and ready.  \r\nEnter \"help\" for a list of commands.");
            while (Online)
            {
                string input = Console.ReadLine();
                if (!Online) break;
                
            }
        }
        public void HandleCommand(string input)
        {
            switch (input)
            {
                case "connected":
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "There are {0} connected clients.", Clients.Count);
                    if (Clients.Count > 0)
                    {
                        MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Connected Clients:");
                        foreach (Client c in Clients)
                        {
                            MinecraftModUpdater.Logger.Log(Logger.Level.Info, c.ToString());
                        }
                    }
                    break;
                case "exit":
                case "stop":
                    foreach (Client c in Clients)
                    {
                        Packet.Send(new MetadataPacket { SData = new string[] { "shutdown", "The Server is shutting down." } }, c.PacketHandler.Stream);
                    }
                    if (Clients.Count > 0) MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Waiting for {0} clients to exit.", Clients.Count);
                    while (Clients.Count > 0) Thread.Sleep(500);
                    Dispose();
                    break;
                case "populate":
                    foreach (string s in Directory.GetFiles(Config.ModsPath + "/mods"))
                    {
                        Mod m = new Mod
                        {
                            Author = "null",
                            Description = "",
                            FileSize = 0,
                            ModFile = "mods/" + Path.GetFileName(s),
                            ModName = Path.GetFileName(s),
                            BlacklistedUsers = new List<string>(),
                            WhitelistedUsers = new List<string>(),
                            PostDownloadCLI = new string[0],
                            Identifier = Extras.GenerateHashFromString(Path.GetFileName(s)),
                            ConfigFile = Config.ModsPath + "/xml/" + Path.GetFileName(s) + ".xml",
                            RequiredMods = new List<Mod>()
                        };
                        m.Save();
                    }
                    Mods.Clear();
                    ModImages.Clear();
                    LoadMods();
                    break;
                case "help":
                case "?":
                default:
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "exit, stop - Safely stops the update server after all clients exit.");
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "connected - Shows a list of connected clients.");
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "populate - Automagicly reads all of the files in the mods folder and creates XML files for them.");
                    break;
            }
        }
        ~Server()
        {
            Dispose();
        }
    }
}
