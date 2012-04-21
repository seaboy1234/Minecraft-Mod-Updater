//    File:        Packet.cs
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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using ModUpdater.Utility;

namespace ModUpdater.Net
{
    public abstract class Packet
    {
        public const int PROTOCOL_VERSION = 5;
        /// <summary>
        /// Reads this packet from the stream.
        /// </summary>
        /// <param name="s"></param>
        public abstract void Read(ModUpdaterNetworkStream s);
        /// <summary>
        /// Writes this packet to the stream.
        /// </summary>
        /// <param name="s"></param>
        public abstract void Write(ModUpdaterNetworkStream s);

        //Static Methods
        public static Dictionary<Type, PacketId> Map = new Dictionary<Type, PacketId>();
        private static Packet LastSent;
        private static bool Busy = false;
        /// <summary>
        /// Reads the next packet in the stream.
        /// </summary>
        /// <param name="Stream">The stream to read from</param>
        /// <returns>A fully read packet.</returns>
        public static Packet ReadPacket(ModUpdaterNetworkStream Stream)
        {
            Type Packet = null;
            Packet p = null;
            try
            {
                PacketId id = (PacketId)Stream.ReadNetworkByte();
                if (!Map.ContainsValue(id))
                {
                    Stream.Flush();
                    return null;
                }
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Read packet {0}", id.ToString()));
                foreach (var v in Map)
                {
                    if (v.Value == id)
                    {
                        Packet = v.Key;
                    }
                }
                p = (Packet)Packet.GetConstructor(new Type[] { }).Invoke(null);
                p.Read(Stream);
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
            return p;
        }
        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="p">The packet to send</param>
        /// <param name="Stream">The stream to send it on.</param>
        public static void Send(Packet p, ModUpdaterNetworkStream Stream)
        {
            while (Busy) ;
            Busy = true;
            try
            {
                PacketId id = GetPacketId(p);
                Stream.WriteNetworkByte((byte)id);
                p.Write(Stream);
                LastSent = p;
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Sent packet {0}", id.ToString()));
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); Console.WriteLine(e); }
            Busy = false;
        }
        public static void ResendLast(ModUpdaterNetworkStream Stream)
        {
            Send(LastSent, Stream);
        }
        /// <summary>
        /// Gets the ID of a packet.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static PacketId GetPacketId(Packet p)
        {
            PacketId id = (PacketId)255;
            if (p == null) return id;
            if (Map.TryGetValue(p.GetType(), out id))
                return id;
            throw new KeyNotFoundException();
        }
        static Packet()
        {
            Map = new Dictionary<Type, PacketId>
            {
                {typeof(HandshakePacket), PacketId.Handshake},
                {typeof(RequestModPacket), PacketId.RequestMod},
                {typeof(FilePartPacket), PacketId.FilePart},
                {typeof(ModInfoPacket), PacketId.ModInfo},
                {typeof(MetadataPacket), PacketId.Metadata},
                {typeof(ModListPacket), PacketId.ModList},
                {typeof(EncryptionStatusPacket), PacketId.EncryptionStatus},
                {typeof(NextDownloadPacket), PacketId.NextDownload},
                {typeof(AllDonePacket), PacketId.AllDone},
                {typeof(LogPacket), PacketId.Log},
                {typeof(DisconnectPacket), PacketId.Disconnect},
                {typeof(ImagePacket), PacketId.Image},
                {typeof(ServerListPacket), PacketId.ServerList},
                {typeof(AdminFileInfoPacket), PacketId.AdminFileInfo}
            };
        }
    }
    #region Packet Classes
    public class HandshakePacket : Packet
    {
        public SessionType Type { get; set; }
        public string Version { get; private set; }
        //Client
        public string Username { get; set; }

        //Server
        public string Address { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            Type = (SessionType)s.ReadNetworkByte();
            Version = s.ReadString();
            switch (Type)
            {
                case SessionType.Client:
                    Username = s.ReadString();
                    break;
                case SessionType.Server:
                    Address = s.ReadString();
                    Name = s.ReadString();
                    Port = s.ReadInt();
                    break;
                case SessionType.Admin:
                    Username = s.ReadString();
                    break;
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteNetworkByte((byte)Type);
            s.WriteString(MinecraftModUpdater.Version);
            switch(Type)
            {
                case SessionType.Client:
                    s.WriteString(Username);
                    break;
                case SessionType.Server:
                    s.WriteString(Address);
                    s.WriteString(Name);
                    s.WriteInt(Port);
                    break;
                case SessionType.Admin:
                    s.WriteString(Username);
                    break;
            }
        }
        public enum SessionType : byte
        {
            Client,
            Server,
            ServerList,
            Admin
        }
    }
    public class RequestModPacket : Packet
    {
        public string FileName { get; set; }
        public RequestType Type { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Type = (RequestType)s.ReadNetworkByte();
            FileName = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteNetworkByte((byte)Type);
            s.WriteString(FileName);
        }
        public enum RequestType : byte
        {
            Info,
            Download,
            Config
        }
    }
    public class FilePartPacket : Packet
    {
        public int Index { get; set; }
        public byte[] Part { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Index = s.ReadInt();
            Part = new byte[s.ReadInt()];
            Part = s.ReadBytes(Part.Length);
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(Index);
            s.WriteInt(Part.Length);
            s.WriteBytes(Part);
        }
    }
    public class ModInfoPacket : Packet
    {
        public string Author { get; set; }
        public string ModName { get; set; }
        public string File { get; set; }
        public string Hash { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Author = s.ReadString();
            ModName = s.ReadString();
            File = s.ReadString();
            Hash = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(Author);
            s.WriteString(ModName);
            s.WriteString(File);
            s.WriteString(Hash);
        }
    }
    public class ModListPacket : Packet
    {
        public string[] Mods { get; set; }
        public int Length { get; private set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Length = s.ReadInt();
            Mods = new string[Length];
            for (int i = 0; i < Length; i++)
            {
                Mods[i] = s.ReadString();
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            Length = Mods.Length;
            s.WriteInt(Length);
            for (int i = 0; i < Length; i++)
            {
                s.WriteString(Mods[i]);
            }
        }
    }
    public class EncryptionStatusPacket : Packet
    {

        public bool Encrypt { get; set; }
        public byte[] EncryptionKey { get; set; }
        public byte[] EncryptionIV { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Encrypt = s.ReadBoolean();
            EncryptionKey = s.ReadBytes(32);
            EncryptionIV = s.ReadBytes(16);
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteBoolean(Encrypt);
            s.WriteBytes(EncryptionKey);
            s.WriteBytes(EncryptionIV);
        }
    }
    public class NextDownloadPacket : Packet
    {
        public string ModName { get; set; }
        public string FileName { get; set; }
        public int Length { get; set; }
        public int ChunkSize { get; set; }
        public string[] PostDownloadCLI { get; set; }
        
        public override void Read(ModUpdaterNetworkStream s)
        {
            ModName = s.ReadString();
            FileName = s.ReadString();
            Length = s.ReadInt();
            ChunkSize = s.ReadInt();
            int i = s.ReadInt();
            PostDownloadCLI = new string[i];
            for (int j = 0; j < i; j++)
            {
                PostDownloadCLI[j] = s.ReadString();
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(ModName);
            s.WriteString(FileName);
            s.WriteInt(Length);
            s.WriteInt(ChunkSize);
            s.WriteInt(PostDownloadCLI.Length);
            foreach (string l in PostDownloadCLI)
            {
                s.WriteString(l);
            }
        }
    }
    public class AllDonePacket : Packet
    {
        public string File { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            File = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(File);
        }
    }
    public class MetadataPacket : Packet
    {
        public string[] SData { get; set; }
        public int[] IData { get; set; }
        public float[] FData { get; set; }
        public bool[] BData { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            int i = s.ReadInt();
            int j = s.ReadInt();
            int k = s.ReadInt();
            int l = s.ReadInt();
            SData = new string[i];
            IData = new int[j];
            FData = new float[k];
            if (i > 0)
                for (int h = 0; h < i; h++)
                {
                    SData[h] = s.ReadString();
                }
            if (j > 0)
                for (int h = 0; h < j; h++)
                {
                    IData[h] = s.ReadInt();
                }
            if (k > 0)
                for (int h = 0; h < k; h++)
                {
                    FData[h] = s.ReadFloat();
                }
            if (l > 0)
                for (int h = 0; h < k; h++)
                {
                    BData[h] = s.ReadBoolean();
                }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            if (SData == null) s.WriteInt(0);
            else s.WriteInt(SData.Length);
            if (IData == null) s.WriteInt(0);
            else s.WriteInt(IData.Length);
            if (FData == null) s.WriteInt(0);
            else s.WriteInt(FData.Length);
            if (BData == null) s.WriteInt(0);
            else s.WriteInt(BData.Length);
            try
            {
                if (SData.Length > 0)
                    foreach (string str in SData)
                    {
                        s.WriteString(str);
                    }
            }
            catch (NullReferenceException) { } //The data is null.  What CAN we do?
            try
            {
                if (IData.Length > 0)
                    foreach (int i in IData)
                    {
                        s.WriteInt(i);
                    }
            }
            catch (NullReferenceException) { } 
            try
            {
                if (FData.Length > 0)
                    foreach (float f in FData)
                    {
                        s.WriteFloat(f);
                    }
            }
            catch (NullReferenceException) { }
            try
            {
                if (BData.Length > 0)
                    foreach (bool b in BData)
                    {
                        s.WriteBoolean(b);
                    }
            }
            catch (NullReferenceException) { }
        }
    }

    public class LogPacket : Packet
    {
        public int Length { get; private set; }
        public string[] LogMessages { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Length = s.ReadInt();
            LogMessages = new string[Length];
            for (int i = 0; i < Length; i++)
            {
                LogMessages[i] = s.ReadString();
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            Length = LogMessages.Length;
            s.WriteInt(Length);
            for (int i = 0; i < Length; i++)
            {
                s.WriteString(LogMessages[i]);
            }
        }
    }
    /// <summary>
    /// Sent when the client disconnects.  This packet contains NO data.
    /// </summary>
    public class DisconnectPacket : Packet
    {
        public override void Read(ModUpdaterNetworkStream s)
        {
            
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            
        }
    }
    public class ImagePacket : Packet
    {
        public ImageType Type { get; set; }
        public string ShowOn { get; set; }
        public byte[] Image { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Type = (ImageType)((byte)s.ReadNetworkByte()); //Stupid ReadNetworkByte().  It's in NetworkStream and thus pretty much out of my control.
            ShowOn = s.ReadString();
            int l = s.ReadInt();
            Image = s.ReadBytes(l);
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteNetworkByte((byte)Type);
            s.WriteString(ShowOn);
            s.WriteInt(Image.Length);
            s.WriteBytes(Image);
        }
        public enum ImageType : byte
        {
            Background,
            Mod
        }
    }
    public class ServerListPacket : Packet
    {
        public string[] Servers { get; set; }
        public string[] Locations { get; set; }
        public int[] Ports { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            int i = s.ReadInt();
            Servers = new string[i];
            Locations = new string[i];
            Ports = new int[i];
            for (int j = 0; j < i; j++)
            {
                Servers[j] = s.ReadString();
                Locations[j] = s.ReadString();
                Ports[j] = s.ReadInt();
            }

        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(Servers.Length);
            for (int i = 0; i < Servers.Length; i++)
            {
                s.WriteString(Servers[i]);
                s.WriteString(Locations[i]);
                s.WriteInt(Ports[i]);
            }

        }
    }
    public class AdminFileInfoPacket : Packet
    {
        public string ConfigName { get; set; }
        public string[] ConfigFile { get; set; }
        public int FileLength { get; set; }
        public int FileChunkSize { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            ConfigName = s.ReadString();
            int len = s.ReadInt();
            ConfigFile = new string[len];
            for (int i = 0; i < len; i++)
            {
                ConfigFile[i] = s.ReadString();
            }
            FileLength = s.ReadInt();
            FileChunkSize = s.ReadInt();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(ConfigName);
            s.WriteInt(ConfigFile.Length);
            for (int i = 0; i < ConfigFile.Length; i++)
            {
                s.WriteString(ConfigFile[i]);
            }
            s.WriteInt(FileLength);
            s.WriteInt(FileChunkSize);
        }
    }
    #endregion
    #region Exceptions
    public class PacketException : Exception
    {
        public PacketException() : base() { }
        public PacketException(string message) : base(message) { }
    }
    #endregion
}
