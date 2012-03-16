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
        private TcpListener listen;
        private bool Online;
        public Server()
        {
            Servers = new List<Slave>();
            Online = false;
            listen = new TcpListener(IPAddress.Any, Properties.Settings.Default.Port);
        }
        public void Start() 
        {
            Online = true;
            listen.Start();
        }
        public void Stop()
        {
            Online = false;
            listen.Stop();
        }
        public void Listen()
        {
            while (Online)
            {
                Socket s = listen.AcceptSocket();
                TaskManager.AddAsyncTask(delegate
                {
                    PacketHandler ph = new PacketHandler(s);
                    ph.Handshake += new PacketEvent<HandshakePacket>(delegate(HandshakePacket p)
                        {
                            if (p.Type == HandshakePacket.SessionType.Server)
                            {
                                Servers.Add(new Slave(p, ph));
                                return;
                            }
                            string[] srvs = new string[Servers.Count];
                            string[] addrs = new string[Servers.Count];
                            int[] ports = new int[Servers.Count];
                            for(int i = 0; i < Servers.Count; i++)
                            {
                                srvs[i] = Servers[i].Name;
                                addrs[i] = Servers[i].Address.ToString();
                                ports[i] = Servers[i].Port;
                            }
                            Packet.Send(new ServerListPacket { Servers = srvs, Locations = addrs, Ports = ports }, ph.Stream);
                            ph.Stop();
                            ph = null;
                        });
                });
            }
        }
    }
}
