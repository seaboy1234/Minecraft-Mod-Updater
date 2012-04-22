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
using ModUpdater.Utility;
using ModUpdater.Net;

namespace ModUpdater.Server.Master
{
    class Server
    {
        public List<Slave> Servers { get; private set; }
        private List<Client> clients;
        private TcpListener listen;
        private bool Online;
        public Server()
        {
            Servers = new List<Slave>();
            clients = new List<Client>();
            Online = false;
            listen = new TcpListener(IPAddress.Any, Properties.Settings.Default.Port);
        }
        public void Start() 
        {
            Online = true;
            listen.Start();
            Listen();
        }
        public void Stop()
        {
            Online = false;
            listen.Stop();
        }
        public void Listen()
        {
            Console.WriteLine("Master Server at {0} is online and listening.", Properties.Settings.Default.Port);
            TaskManager.AddAsyncTask(delegate
            {
                string ver;
                if (Extras.CheckForUpdate("master", Program.Version, out ver))
                {
                    Console.WriteLine("Version {0} is now available for Minecraft Mod Updater.", ver);
                }
            });
            while (Online)
            {
                Socket s = listen.AcceptSocket();
                TaskManager.AddAsyncTask(delegate
                {
                    Client c = new Client(this, new PacketHandler(s));
                    c.ClientDisconnected += delegate
                    {
                        c = null;
                    };
                });
            }
        }
        class Client
        {
            private Server Server;
            private PacketHandler ph;
            public event EventHandler ClientDisconnected = delegate { };
            public Client(Server s, PacketHandler p)
            {
                Server = s;
                ph = p;
                ph.Start();
                ph.RegisterPacketHandler(PacketId.Handshake, Handle);
            }
            private void Handle(Packet pa)
            {
                HandshakePacket p = pa as HandshakePacket;
                if (p.Type == HandshakePacket.SessionType.Server)
                {
                    Slave sl = new Slave(p, ph);
                    Server.Servers.Add(sl);
                    Console.WriteLine("New Server: " + sl.ToString());
                    return;
                }
                string[] srvs = new string[Server.Servers.Count];
                string[] addrs = new string[Server.Servers.Count];
                int[] ports = new int[Server.Servers.Count];
                for (int i = 0; i < Server.Servers.Count; i++)
                {
                    srvs[i] = Server.Servers[i].Name;
                    addrs[i] = Server.Servers[i].Address.ToString();
                    ports[i] = Server.Servers[i].Port;
                }
                Packet.Send(new ServerListPacket { Servers = srvs, Locations = addrs, Ports = ports }, ph.Stream);
                ph.Stop();
                ClientDisconnected(this, EventArgs.Empty);
            }
        }
    }
}
