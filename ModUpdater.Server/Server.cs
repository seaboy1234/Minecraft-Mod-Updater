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
        bool Online { get; set; }
        public Server()
        {
            Config.Load();
            Mods = new List<Mod>();
            Clients = new List<Client>();
            TcpServer = new TcpListener(IPAddress.Any, Config.Port);
            ModImages = new Dictionary<Mod, Image>();
            SelfUpdate();
            foreach (string s in Directory.GetFiles(Config.ModsPath + "/xml"))
            {
                Mods.Add(new Mod(s));
            }
            foreach (Mod m in Mods)
            {
                if (File.Exists(Config.ModsPath + "/ModAssets/" + Path.GetFileName(m.ModFile) + ".png"))
                {
                    ModImages.Add(m, Image.FromFile(Config.ModsPath + "/ModAssets/" + Path.GetFileName(m.ModFile) + ".png"));
                }
            }
            if (File.Exists(Config.ModsPath + "/assets/server_background.png"))
            {
                BackgroundImage = Image.FromFile(Config.ModsPath + "/assets/server_background.png");
            }
            Console.WriteLine("Registered {0} mods", Mods.Count);
        }
        private void SelfUpdate()
        {
            if (Config.ModsPath != ".")
            {
                try
                {
                    if (!Directory.Exists(Config.ModsPath)) Directory.CreateDirectory(Config.ModsPath);
                    if (Directory.Exists("mods")) Directory.Move("mods", Config.ModsPath + "/mods");
                    if (Directory.Exists("xml")) Directory.Move("xml", Config.ModsPath + "/xml");
                    if (Directory.Exists("ModAssets")) Directory.Move("ModAssets", Config.ModsPath + "/ModAssets");
                }
                catch { }
            }
            if (!Directory.Exists(Config.ModsPath + "/mods")) Directory.CreateDirectory(Config.ModsPath + "/mods");
            if (!Directory.Exists(Config.ModsPath + "/xml")) Directory.CreateDirectory(Config.ModsPath + "/xml");
            if (!Directory.Exists(Config.ModsPath + "/assets")) Directory.CreateDirectory(Config.ModsPath + "/assets");
            if (!Directory.Exists(Config.ModsPath + "/ModAssets")) Directory.CreateDirectory(Config.ModsPath + "/ModAssets");
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
            Console.WriteLine("Server IP Address is: " + Address.ToString());
            TcpServer.Start();
            Online = true;
            TaskManager.AddAsyncTask(delegate { SimpleConsoleImputHandler(); });
            TaskManager.AddDelayedAsyncTask(delegate
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
            }, 1000);
            TaskManager.AddAsyncTask(delegate
            {
                string ver;
                if (Extras.CheckForUpdate("server", Program.Version, out ver))
                {
                    Console.WriteLine("Version {0} is now available for Minecraft Mod Updater.", ver);
                }
            });
            Receive();
        }
        public void Dispose()
        {
            TcpServer.Stop();
            Online = false;
            Config.Save();
        }
        public void Receive()
        {
            while (Online)
            {
                try
                {
                    Socket s = TcpServer.AcceptSocket();
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
                Clients.Add(c);
                c.StartListening();
                Thread.Sleep(1000);
                c.ClientDisconnected += delegate
                {
                    Clients.Remove(c);
                };
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
        public void SimpleConsoleImputHandler()
        {
            Console.WriteLine("Simple Console Input Handler is online and ready.  \r\nEnter \"help\" for a list of commands.");
            while (Online)
            {
                string input = Console.ReadLine();
                if (!Online) break;
                switch (input)
                {
                    case "connected":
                        Console.WriteLine("There are {0} connected clients.", Clients.Count);
                        if (Clients.Count > 0)
                        {
                            Console.WriteLine("Connected Clients:");
                            foreach (Client c in Clients)
                            {
                                Console.WriteLine(c.ToString());
                            }
                        }
                        break;
                    case "exit":
                    case "stop":
                        foreach (Client c in Clients)
                        {
                            Packet.Send(new MetadataPacket { SData = new string[] { "shutdown", "The Server is shutting down." } }, c.PacketHandler.Stream);
                        }
                        if (Clients.Count > 0) Console.WriteLine("Waiting for {0} clients to exit.", Clients.Count);
                        while (Clients.Count > 0) ;
                        Dispose();
                        break;
                    case "populate":
                        foreach(string s in Directory.GetFiles(Config.ModsPath + "/mods"))
                        {
                            string[] file = new string[] {
                                "<Mod>",
                                "    <Name>" + Path.GetFileName(s) + "</Name>",
                                "    <Author>null</Author>",
                                "    <File>mods/" + Path.GetFileName(s) + "</File>",
                                "    <PostDownload>",
                                "        <Action>echo Example</Action>",
                                "    </PostDownload>",
                                "</Mod>" };
                            File.WriteAllLines(Config.ModsPath + "/xml/" + Path.GetFileName(s) + ".xml", file);
                        }
                        Mods.Clear();
                        ModImages.Clear();
                        foreach (string s in Directory.GetFiles(Config.ModsPath + "/xml"))
                        {
                            Mods.Add(new Mod(s));
                        }
                        foreach (Mod m in Mods)
                        {
                            if (File.Exists(Config.ModsPath + "/ModAssets/" + Path.GetFileName(m.ModFile) + ".png"))
                            {
                                ModImages.Add(m, Image.FromFile(Config.ModsPath + "/ModAssets/" + Path.GetFileName(m.ModFile) + ".png"));
                            }
                        }
                        Console.WriteLine("Registered {0} mods", Mods.Count);
                        break;
                    case "help":
                    case "?":
                    default:
                        Console.WriteLine("exit, stop - Safely stops the update server after all clients exit.");
                        Console.WriteLine("connected - Shows a list of connected clients.");
                        Console.WriteLine("populate - Automagicly reads all of the files in the mods folder and creates XML files for them.");
                        break;
                }
            }
        }
    }
}
