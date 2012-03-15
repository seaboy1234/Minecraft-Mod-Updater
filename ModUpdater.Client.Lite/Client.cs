//    File:        Client.cs
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

namespace ModUpdater.Client.Lite
{
    public class Client
    {
        private List<Mod> mods;
        private ClientPacketHandler ph;
        private Socket s;
        private ModUpdaterNetworkStream m;
        public bool Start()
        {
            mods = new List<Mod>();
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ph = new ClientPacketHandler(s);
            TaskManager.AddSyncTask(delegate { s.Connect(IPAddress.Parse(Properties.Settings.Default.Server), 4713); });
            if (!s.Connected)
            {
                return false;
            }
            ph.Start();
            m = ph.Stream;
            ph.ModList += new PacketEvent<ModListPacket>(ph_ModList);
            ph.ModInfo += new PacketEvent<ModInfoPacket>(ph_ModInfo);
            return true;
        }
        public Mod GetMod(string name)
        {
            foreach (Mod m in mods)
            {
                if (m.Name.ToLower() == name.ToLower())
                    return m;
            }
            return null;
        }
        public Mod[] GetMods()
        {
            return mods.ToArray();
        }
        void ph_ModInfo(ModInfoPacket p)
        {
            mods.Add(new Mod(p));
        }

        void ph_ModList(ModListPacket p)
        {
            foreach (string s in p.Mods)
            {
                Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Info, FileName = s }, m);
            }
        }
    }
}
