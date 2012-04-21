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
using System.Threading;

namespace ModUpdater.Server
{
    class Client
    {
        public IPEndPoint IPAddress { get; private set; }
        public Server Server {get; private set;}
        public string ClientID { get; private set; }
        public PacketHandler PacketHandler { get { return ph; } }
        public bool Admin { get; private set; }
        public event EventHandler ClientDisconnected = delegate { };
        private PacketHandler ph;
        private List<Mod> allowedMods;
        public Client(Socket s, Server sv)
        {
            ph = new PacketHandler(s);
            ph.RegisterPacketHandler(PacketId.Handshake, RegisterClient);
            ph.RegisterPacketHandler(PacketId.Log, HandleLog);
            ph.RegisterPacketHandler(PacketId.RequestMod, RetreveMod);
            ph.RegisterPacketHandler(PacketId.Disconnect, HandleDisconnect);
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
            ph.RemovePacketHandler(PacketId.Handshake);
            ph.RemovePacketHandler(PacketId.RequestMod);
            ph.RemovePacketHandler(PacketId.Log);
            ph.RemovePacketHandler(PacketId.Disconnect);
            ph.Stop();
        }
        public void RetreveMod(Packet pa)
        {
            RequestModPacket p = pa as RequestModPacket;
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
        internal void RegisterClient(Packet pa)
        {
            HandshakePacket p = pa as HandshakePacket;
            if (p.Type == HandshakePacket.SessionType.ServerList)
            {
                Packet.Send(new DisconnectPacket(), ph.Stream);
                ph.Stop();
                return;
            }
            else if (p.Type == HandshakePacket.SessionType.Admin)
            {
                if (!Server.Administrators.Contains(p.Username))
                {
                    Packet.Send(new MetadataPacket { SData = new string[] { "admin_login" }, BData = new bool[] { false } }, ph.Stream);
                    Packet.Send(new DisconnectPacket(), ph.Stream);
                    ph.Stop();
                    return;
                }
                Packet.Send(new MetadataPacket { SData = new string[] { "admin_login" }, BData = new bool[] { true } }, ph.Stream);
                Admin = true;
            }
            ClientID = p.Username;
            if (MinecraftModUpdater.Version != p.Version)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "require_version", MinecraftModUpdater.Version } }, ph.Stream);
                Thread.Sleep(1000);
                ph.Stop();
                return;
            }
            Console.WriteLine("Client {0} connected. ({1})", ClientID, IPAddress.Address);
            Packet.Send(new EncryptionStatusPacket { Encrypt = true, EncryptionIV = ph.Stream.IV, EncryptionKey = ph.Stream.Key }, ph.Stream);
            ph.Stream.Encrypted = true;
            allowedMods = Server.Mods;
            Packet.Send(new MetadataPacket { SData = new string[] { "server_name", Config.ServerName }, FData = new float[] { 24.0f } }, ph.Stream);
            if (!Admin)
            {
                for (int i = 0; i < Server.Mods.Count; i++)
                {
                    if (Server.Mods[i].WhitelistedUsers.Contains(ClientID) || !Server.Mods[i].BlacklistedUsers.Contains(ClientID))
                        allowedMods.Add(Server.Mods[i]);
                    else Console.WriteLine("NOT SENDING: " + Server.Mods[i].ModName);
                }
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
            }
            string[] mods = new string[allowedMods.Count];
            for (int i = 0; i < allowedMods.Count; i++)
            {
                mods[i] = allowedMods[i].ModFile;
            }
            Packet.Send(new ModListPacket { Mods = mods }, ph.Stream);
        }

        internal void HandleLog(Packet pa)
        {
            LogPacket p = pa as LogPacket;
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
