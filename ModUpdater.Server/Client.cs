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
using System.Net;
using System.Net.Sockets;
using System.IO;
using ModUpdater.Net;
using ModUpdater.Utility;

namespace ModUpdater.Server
{
    class Client
    {
        public IPEndPoint IPAddress { get; private set; }
        public Server Server {get; private set;}
        public string ClientID { get; private set; }
        public PacketHandler PacketHandler { get { return ph; } }
        private PacketHandler ph;
        private PacketHandler ph2;
        private List<Mod> allowedMods;
        public Client(Socket s, Server sv)
        {
            ph = new PacketHandler(s);
            ph.Handshake += new PacketEvent<HandshakePacket>(RegisterClient);
            ph.RequestMod += new PacketEvent<RequestModPacket>(RetreveMod);
            ph.Log += new PacketEvent<LogPacket>(HandleLog);
            ph.Disconnect += new PacketEvent<Packet>(HandleDisconnect);
            Server = sv;
            IPAddress = (IPEndPoint)s.RemoteEndPoint;
            allowedMods = new List<Mod>();
        }
        public void StartListening()
        {
            ph.Start();
        }
        public void Dispose()
        {
            ph.Stop();
        }
        void HandleDisconnect(Packet p)
        {
            ph.Handshake -= new PacketEvent<HandshakePacket>(RegisterClient);
            ph.RequestMod -= new PacketEvent<RequestModPacket>(RetreveMod);
            ph.Log -= new PacketEvent<LogPacket>(HandleLog);
            ph.Disconnect -= new PacketEvent<Packet>(HandleDisconnect);
            ph.Stop();
        }
        public void RetreveMod(RequestModPacket p)
        {
            Mod mod = Server.Mods.Find(new Predicate<Mod>(delegate(Mod m)
            {
                if (m.ModFile == p.FileName)
                    return true;
                return false;
            }));
            Console.WriteLine("Client {0} requested {1}.", ClientID, mod.ModName);
            switch (p.Type)
            {
                case RequestModPacket.RequestType.Info:
                    Packet.Send(new ModInfoPacket { Author = mod.Author, File = mod.ModFile, ModName = mod.ModName, Hash = Extras.GenerateHash(Config.ModsPath + "\\" + mod.ModFile) }, ph.Stream);
                    break;
                case RequestModPacket.RequestType.Download:
                    byte[] file = File.ReadAllBytes(Config.ModsPath + "\\" + mod.ModFile);
                    List<List<byte>> abyte = new List<List<byte>>();
                    int k = 0;
                    for (int i = 0; i < file.Length; i+= 2048)
                    {
                        abyte.Add(new List<byte>());
                        for (int j = i; j < i + 2048; j++)
                        {
                            if(file.Length > j)
                                abyte[k].Add(file[j]);
                        }
                        k++;
                    }
                    Packet.Send(new NextDownloadPacket { ModName = mod.ModName, FileName = mod.ModFile, Length = file.Length, PostDownloadCLI = mod.PostDownloadCLI, ChunkSize = abyte.Count }, ph.Stream);
                    int l = 0;
                    for (int h = 0; h < abyte.Count; h++)
                    {
                        byte[] b = abyte[h].ToArray();
                        b = ph.Stream.EncryptBytes(b);
                        Packet.Send(new FilePartPacket { Part = b, Index = l }, ph.Stream);
                        l += abyte[h].Count;
                    }
                    Packet.Send(new AllDonePacket { File = mod.ModFile }, ph.Stream);
                    break;
            }
        }
        public void BeginDownloadProcess()
        {

        }
        internal void RegisterClient(HandshakePacket p)
        {
            if (p.Type == HandshakePacket.SessionType.ServerList)
            {
                Packet.Send(new DisconnectPacket(), ph.Stream);
                ph.Stop();
                return;
            }
            ClientID = p.Username;
            if (Packet.PROTOCOL_VERSION != p.Version)
            {
                ph.Stop();
                return;
            }
            Console.WriteLine("Client {0} connected. ({1})", ClientID, IPAddress.Address);
            Packet.Send(new EncryptionStatusPacket { Encrypt = true, EncryptionIV = ph.Stream.IV, EncryptionKey = ph.Stream.Key }, ph.Stream);
            ph.Stream.Encrypted = true;
            for (int i = 0; i < Server.Mods.Count; i++)
            {
                if(Server.Mods[i].WhitelistedUsers.Contains(ClientID) || !Server.Mods[i].BlacklistedUsers.Contains(ClientID))
                    allowedMods.Add(Server.Mods[i]);
                else Console.WriteLine("NOT SENDING: " + Server.Mods[i].ModName);
            }
            Packet.Send(new MetadataPacket { SData = new string[] { "server_name", Config.ServerName }, FData = new float[] { 24.0f } }, ph.Stream);
            Packet.Send(new MetadataPacket { SData = new string[] { "splash_display", "Downloading Assets..." } }, ph.Stream);
            if (Server.BackgroundImage != null)
            {
                byte[] b = ph.Stream.EncryptBytes(Extras.BytesFromImage(Server.BackgroundImage));
                Packet.Send(new ImagePacket { Type = ImagePacket.ImageType.Background, ShowOn = "", Image = b }, ph.Stream);
            }
            foreach (var v in Server.ModImages)
            {
                byte[] b = ph.Stream.EncryptBytes(Extras.BytesFromImage(v.Value));
                Packet.Send(new ImagePacket { Type = ImagePacket.ImageType.Mod, ShowOn = v.Key.ModFile, Image = b }, ph.Stream);
            }
            string[] mods = new string[allowedMods.Count];
            for (int i = 0; i < allowedMods.Count; i++)
            {
                mods[i] = allowedMods[i].ModFile;
            }
            Packet.Send(new ModListPacket { Mods = mods }, ph.Stream);
        }

        internal void HandleLog(LogPacket p)
        {
            string fullpath = @"clientlogs\" + IPAddress.Address.ToString() + @"\log_" + DateTime.Now.ToString().Replace(":", "-").Replace("/", ".") + ".txt";
            if (!Directory.Exists(Path.GetDirectoryName(fullpath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            }
            try
            {
                File.WriteAllLines(fullpath, p.LogMessages);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
        public override string ToString()
        {
            return IPAddress.Address.ToString();
        }
        public bool Connected { get { return !ph.Stream.Disposed; } }
    }
}
