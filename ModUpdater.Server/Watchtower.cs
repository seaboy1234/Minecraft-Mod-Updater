//    File:        Watchtower.cs
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
using ModUpdater.Net;

namespace ModUpdater.Server
{
    class Watchtower
    {
        private List<Client> clients;
        private Server server;

        public Watchtower(Server server)
        {
            this.server = server;
            clients = new List<Client>();
        }

        public void BroadcastToTowers(string message)
        {
            foreach (Client c in clients)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "message", "mbox", message } }, c.PacketHandler.Stream);
            }
        }
        public void BroadcastToTower(Client c, string message)
        {
            Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "message", "mbox", message } }, c.PacketHandler.Stream);
        }
        public void LogToTowers(string message)
        {
            foreach (Client c in clients)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "message", "log", message } }, c.PacketHandler.Stream);
            }
        }
        public void ClientStatus(bool joined, Client c)
        {
            string msg = joined ? "join" : "leave";
            foreach (Client cl in clients)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "client_status", c.ClientID, msg } }, cl.PacketHandler.Stream);
            }
        }
        public void RegisterWatchtowerUser(Client c)
        {
            LogToTowers(c.ClientID + " has entered Watchtower.");
            clients.Add(c);
        }
        public void RemoveWatchtowerUser(Client c)
        {
            clients.Remove(c);
            LogToTowers(c.ClientID + " has left Watchtower.");
        }
        public void HandleWatchtowerMessage(Client c, MetadataPacket p)
        {
            if (!c.Admin)
            {
                return;
            }
            switch (p.SData[1])
            {
                case "status":
                    if (p.SData[2] == "enable")
                    {
                        RegisterWatchtowerUser(c);
                    }
                    else if (p.SData[2] == "disable")
                    {
                        RemoveWatchtowerUser(c);
                    }
                    break;
                case "kick":
                    foreach (Client cl in server.Clients)
                    {
                        if (cl.ClientID.ToLower() == p.SData[2])
                        {
                            Packet.Send(new MetadataPacket { SData = new string[] { "shutdown", p.SData[3] } }, cl.PacketHandler.Stream);
                        }
                    }
                    break;
                case "command":
                    server.HandleCommand(p.SData[2]);
                    break;
            }
        }
    }
}
