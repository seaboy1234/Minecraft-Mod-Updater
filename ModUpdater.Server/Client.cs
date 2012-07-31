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
        private Mod currentDownload;
        private byte[] downloaded;
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

        void ClientLeft(object sender, EventArgs e)
        {
            HandleDisconnect(null);
        }
        public void StartListening()
        {
            ph.Start();
            ph.Disconnect += ClientLeft;
        }
        void HandleDisconnect(Packet p)
        {
            TaskManager.AddAsyncTask(delegate
            {
                ph.RemovePacketHandler(PacketId.Handshake);
                ph.RemovePacketHandler(PacketId.RequestMod);
                ph.RemovePacketHandler(PacketId.Log);
                ph.RemovePacketHandler(PacketId.Disconnect);
            });
            ph.Disconnect-= ClientLeft;
            ClientDisconnected(this, EventArgs.Empty);
        }
        public void RetreveMod(Packet pa)
        {
            RequestModPacket p = pa as RequestModPacket;
            Mod mod = Server.Mods.Find(new Predicate<Mod>(delegate(Mod m)
            {
                if (m.Identifier == p.Identifier)
                    return true;
                return false;
            }));
            MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Client {0} requested {1}.", ClientID, mod.ModName);
            switch (p.Type)
            {
                case RequestModPacket.RequestType.Info:
                    string[] requiredMods = new string[mod.RequiredMods.Count];
                    for(int i = 0; i < requiredMods.Length; i++)
                    {
                        requiredMods[i] = mod.RequiredMods[i].Identifier;
                    }
                    if (Admin)
                    {
                        Packet.Send(new AdminFileInfoPacket 
                        { 
                            ModName = mod.ModName,
                            Author = mod.Author, 
                            BlacklistedUsers = mod.BlacklistedUsers.ToArray(), 
                            Description = mod.Description, 
                            File = mod.ModFile, 
                            FileSize = mod.FileSize, 
                            Hash = mod.Hash,
                            PostDownload = mod.PostDownloadCLI, 
                            WhitelistedUsers = mod.WhitelistedUsers.ToArray() ,
                            Identifier = mod.Identifier,
                            Optional = mod.Optional,
                            Requires = requiredMods
                        }, ph.Stream);
                        return;
                    }
                    Packet.Send(new ModInfoPacket { Author = mod.Author, File = mod.ModFile, ModName = mod.ModName, Hash = Extras.GenerateHash(Config.ModsPath + "\\" + mod.ModFile), FileSize = mod.FileSize, Description = mod.Description, Identifier = mod.Identifier, Optional = mod.Optional, Requires = requiredMods }, ph.Stream);
                    break;
                case RequestModPacket.RequestType.Download:
                    mod.SendFileTo(this);
                    break;
            }
        }
        internal void RegisterClient(Packet pa)
        {
            HandshakePacket p = pa as HandshakePacket;
            if (p.Type == HandshakePacket.SessionType.ServerList)
            {
                Packet.Send(new DisconnectPacket(), ph.Stream);
                HandleDisconnect(null);
                ph.Stop();
                return;
            }
            else if (p.Type == HandshakePacket.SessionType.Admin)
            {
                if (!Server.Administrators.Contains(p.Username))
                {
                    Packet.Send(new MetadataPacket { SData = new string[] { "admin_login", "false" } }, ph.Stream);
                    Packet.Send(new DisconnectPacket(), ph.Stream);
                    ph.Stop();
                    return;
                }
                Packet.Send(new MetadataPacket { SData = new string[] { "admin_login", "true" } }, ph.Stream);
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
            MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Client {0} connected. ({1})", ClientID, IPAddress.Address);
            Packet.Send(new EncryptionStatusPacket { Encrypt = true, EncryptionIV = ph.Stream.IV, EncryptionKey = ph.Stream.Key }, ph.Stream);
            ph.Stream.Encrypted = true;
            Packet.Send(new MetadataPacket { SData = new string[] { "server_name", Config.ServerName }, FData = new float[] { 24.0f } }, ph.Stream);
            if (!Admin)
            {
                for (int i = 0; i < Server.Mods.Count; i++)
                {
                    if (Server.Mods[i].WhitelistedUsers.Contains(ClientID) || !Server.Mods[i].BlacklistedUsers.Contains(ClientID))
                        allowedMods.Add(Server.Mods[i]);
                    else MinecraftModUpdater.Logger.Log(Logger.Level.Info,"NOT SENDING: " + Server.Mods[i].ModName);
                }
                Packet.Send(new MetadataPacket { SData = new string[] { "splash_display", "Downloading Assets..." } }, ph.Stream);
                if (Server.BackgroundImage != null)
                {
                    byte[] b = Extras.BytesFromImage(Server.BackgroundImage);
                    Packet.Send(new ImagePacket { Type = ImagePacket.ImageType.Background, ShowOn = "", Image = b }, ph.Stream);
                }
                foreach (var v in Server.ModImages)
                {
                    byte[] b = Extras.BytesFromImage(v.Value);
                    Packet.Send(new ImagePacket { Type = ImagePacket.ImageType.Mod, ShowOn = v.Key.ModFile, Image = b }, ph.Stream);
                }
            }
            else
            {
                allowedMods.AddRange(Server.Mods.ToArray());
            }
            string[] mods = new string[allowedMods.Count];
            for (int i = 0; i < allowedMods.Count; i++)
            {
                mods[i] = allowedMods[i].Identifier;
            }
            Packet.Send(new MetadataPacket { SData = new string[] { "splash_display", "Downloading Mod List..." } }, ph.Stream);
            Packet.Send(new ModListPacket { Mods = mods }, ph.Stream);
            MinecraftModUpdater.Logger.Log(Logger.Level.Info,"{0} Logged in.", this.ClientID);
            if (Admin)
            {
                PacketHandler.RegisterPacketHandler(PacketId.AdminFileInfo, UpdateModInfo);
                PacketHandler.RegisterPacketHandler(PacketId.UploadFile, HandleFileUpload);
                PacketHandler.RegisterPacketHandler(PacketId.FilePart, HandleFilePart);
                PacketHandler.RegisterPacketHandler(PacketId.AllDone, HandleCompleteDownload);
            }
            else
            {
                if (Config.ClientVersion != "")
                {
                    Packet.Send(new MetadataPacket { SData = new string[] { "version_downgrade", Config.ClientVersion } }, PacketHandler.Stream);
                }
            }
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
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
        }
        internal void UpdateModInfo(Packet pa)
        {
            AdminFileInfoPacket p = pa as AdminFileInfoPacket;
            Mod m = Server.Mods.Find(new Predicate<Mod>(delegate(Mod mod)
                {
                    if (mod.Identifier == p.Identifier)
                        return true;
                    return false;
                }));
            if (m == null)
            {
                m = new Mod();
                Server.Mods.Add(m);
            }
            else if (m.ModFile != p.File)

            {
                File.Delete(Config.ModsPath + "/xml/" + Path.GetFileName(m.ModFile) + ".xml");
                File.Delete(Config.ModsPath + "/" + m.ModFile);
            }
            m.Author = p.Author;
            m.BlacklistedUsers.Clear();
            m.BlacklistedUsers.AddRange(p.BlacklistedUsers);
            m.Description = p.Description;
            m.FileSize = p.FileSize;
            m.ModFile = p.File;
            m.ModName = p.ModName;
            m.PostDownloadCLI = p.PostDownload;
            m.WhitelistedUsers.Clear();
            m.WhitelistedUsers.AddRange(p.WhitelistedUsers);
            m.ConfigFile = Config.ModsPath + "/xml/" + Path.GetFileName(p.File) + ".xml";
            m.Identifier = p.Identifier;
            m.Optional = p.Optional;
            m.RequiredMods = new List<Mod>();
            foreach (Mod mod in Server.Mods)
            {
                if (p.Requires.Contains(mod.Identifier))
                    m.RequiredMods.Add(mod);
            }
            m.Save();
            MinecraftModUpdater.Logger.Log(Logger.Level.Warning, "{0}({1}) has been updated by {2}", m.ModName, m.Identifier, ClientID);
        }
        internal void HandleFilePart(Packet pa)
        {
            try
            {
                FilePartPacket p = pa as FilePartPacket;
                int i = p.Index;
                foreach (byte b in p.Part)
                {
                    downloaded[i] = b;
                    i++;
                }
            }
            catch(Exception e)
            {
                MinecraftModUpdater.Logger.Log(e);
                Console.ReadLine();
            }
        }
        internal void HandleFileUpload(Packet pa)
        {
            UploadFilePacket p = pa as UploadFilePacket;
            Mod m = Server.Mods.Find(new Predicate<Mod>(delegate(Mod mod)
            {
                if (mod.Identifier == p.Identifier)
                    return true;
                return false;
            }));
            if (m == null)
            {
                m = new Mod();
            }
            if (File.Exists(Config.ModsPath + "/" + m.ModFile))
                File.Delete(Config.ModsPath + "/" + m.ModFile);
            currentDownload = m;
            downloaded = new byte[p.Size];
        }
        internal void HandleCompleteDownload(Packet pa)
        {
            AllDonePacket p = pa as AllDonePacket;
            currentDownload.ReadFile(downloaded);
            currentDownload.Save();
            if (!Directory.Exists(Config.ModsPath + "/" + Path.GetDirectoryName(currentDownload.ModFile))) 
                Directory.CreateDirectory(Config.ModsPath + "/" + Path.GetFileName(currentDownload.ModFile));
            File.WriteAllBytes(Config.ModsPath + "/" + currentDownload.ModFile, downloaded);
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Data transfer complete!");
        }
        public override string ToString()
        {
            return IPAddress.Address.ToString();
        }
        public bool Connected { get { return !ph.Stream.Disposed; } }
    }
}
