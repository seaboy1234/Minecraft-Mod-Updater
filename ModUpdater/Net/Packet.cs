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

        public DateTime Timestamp { get; private set; }
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
        public static Packet LastSent { get { return lastSent; } }
        public static Packet LastRecived { get { return lastRecived; } }
        private static Packet lastSent;
        private static Packet lastRecived;
        private static bool busy = false;
        /// <summary>
        /// Reads the next packet in the stream.
        /// </summary>
        /// <param name="Stream">The stream to read from</param>
        /// <returns>A fully read packet.</returns>
        public static Packet ReadPacket(ModUpdaterNetworkStream Stream)
        {
            Type Packet = null;
            Packet p = null;
            if (Stream.Disposed) return null;
            try
            {
                PacketId id = PacketId.Disconnect;
                try
                {
                     id = (PacketId)Stream.ReadNetworkByte();
                }
                catch (MalformedPacketException) { return null; }
                if (!Map.ContainsValue(id))
                {
                    Stream.Flush();
                    return null;
                }
                MinecraftModUpdater.Logger.Log(Logger.Level.Debug, string.Format("Read packet {0}", id.ToString()));
                foreach (var v in Map)
                {
                    if (v.Value == id)
                    {
                        Packet = v.Key;
                    }
                }
                p = (Packet)Packet.GetConstructor(new Type[] { }).Invoke(null);
                p.Timestamp = DateTime.Now;
                p.Read(Stream);
                lastRecived = p;
            }
            catch (MalformedPacketException e) { throw new MalformedPacketException(e.Message, e); }
            catch (Exception e) { MCModUpdaterExceptionHandler.HandleException(p, e); }
            return p;
        }
        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="p">The packet to send</param>
        /// <param name="Stream">The stream to send it on.</param>
        public static void Send(Packet p, ModUpdaterNetworkStream Stream)
        {
            if (Stream.Disposed) return;
            while (busy) ;
            busy = true;
            try
            {
                PacketId id = GetPacketId(p);
                Stream.WriteNetworkByte((byte)id);
                p.Write(Stream);
                lastSent = p;
                MinecraftModUpdater.Logger.Log(Logger.Level.Debug, string.Format("Sent packet {0}", id.ToString()));
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); MinecraftModUpdater.Logger.Log(e); }
            busy = false;
        }
        public static void ResendLast(ModUpdaterNetworkStream Stream)
        {
            Send(lastSent, Stream);
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
                {typeof(AdminFileInfoPacket), PacketId.AdminFileInfo},
                {typeof(UploadFilePacket), PacketId.UploadFile}
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
        public string Identifier { get; set; }
        public RequestType Type { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            Type = (RequestType)s.ReadNetworkByte();
            Identifier = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteNetworkByte((byte)Type);
            s.WriteString(Identifier);
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
            Part = s.ReadBytes();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(Index);
            s.WriteBytes(Part);
        }
    }
    public class ModInfoPacket : Packet
    {
        public string Author { get; set; }
        public string ModName { get; set; }
        public string File { get; set; }
        public string Hash { get; set; }
        public long FileSize { get; set; }
        public string Description { get; set; }
        public string Identifier { get; set; }
        public bool Optional { get; set; }
        public string[] Requires { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            Author = s.ReadString();
            ModName = s.ReadString();
            File = s.ReadString();
            Hash = s.ReadString();
            FileSize = s.ReadLong();
            Description = s.ReadString();
            Identifier = s.ReadString();
            Optional = s.ReadBoolean();
            Requires = s.ReadStrings();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(Author);
            s.WriteString(ModName);
            s.WriteString(File);
            s.WriteString(Hash);
            s.WriteLong(FileSize);
            s.WriteString(Description);
            s.WriteString(Identifier);
            s.WriteBoolean(Optional);
            s.WriteStrings(Requires);
        }
    }
    public class ModListPacket : Packet
    {
        public string[] Mods { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Mods = s.ReadStrings();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteStrings(Mods);
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
            EncryptionKey = s.ReadBytes();
            EncryptionIV = s.ReadBytes();
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
        public string Identifier { get; set; }
        public int ChunkSize { get; set; }
        public string[] PostDownloadCLI { get; set; }
        
        public override void Read(ModUpdaterNetworkStream s)
        {
            Identifier = s.ReadString();
            ChunkSize = s.ReadInt();
            PostDownloadCLI = s.ReadStrings();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(Identifier);
            s.WriteInt(ChunkSize);
            s.WriteStrings(PostDownloadCLI);
        }
    }
    public class AllDonePacket : Packet
    {
        public string Identifier { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Identifier = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(Identifier);
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
        public string[] LogMessages { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            int l = s.ReadInt();
            LogMessages = new string[l];
            for (int i = 0; i < l; i++)
            {
                LogMessages[i] = s.ReadString();
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            int l = LogMessages.Length;
            s.WriteInt(l);
            for (int i = 0; i < l; i++)
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
            Image = s.ReadBytes();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteNetworkByte((byte)Type);
            s.WriteString(ShowOn);
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
    public class AdminFileInfoPacket : ModInfoPacket
    {
        public string[] BlacklistedUsers { get; set; }
        public string[] WhitelistedUsers { get; set; }
        public string[] PostDownload { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            base.Read(s);
            BlacklistedUsers = s.ReadStrings();
            WhitelistedUsers = s.ReadStrings();
            PostDownload = s.ReadStrings();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            base.Write(s);
            s.WriteStrings(BlacklistedUsers);
            s.WriteStrings(WhitelistedUsers);
            s.WriteStrings(PostDownload);
        }

    }
    public class UploadFilePacket : Packet
    {
        public string Identifier { get; set; }
        public long Size { get; set; }
        public int Parts { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            Identifier = s.ReadString();
            Size = s.ReadLong();
            Parts = s.ReadInt();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(Identifier);
            s.WriteLong(Size);
            s.WriteInt(Parts);
        }
    }
    #endregion
    #region Exceptions
    public class PacketException : Exception
    {
        public PacketException() : base() { }
        public PacketException(string message) : base(message) { }
        public PacketException(string message, Exception ex) : base(message, ex) { }
    }
    #endregion
}
