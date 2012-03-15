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

namespace ModUpdater
{
    public abstract class Packet
    {
        public const int PROTOCOL_VERSION = 4;
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
        public static Dictionary<PacketId, Type> Map = new Dictionary<PacketId, Type>();
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
                PacketId id = (PacketId)Stream.ReadByte();
                if (id == PacketId.Disconnect) return null;
                if (!Map.ContainsKey(id))
                {
                    Stream.Flush();
                    return null;
                }
                Packet = Map[id];
                p = (Packet)Packet.GetConstructor(new Type[] { }).Invoke(null);
                p.Read(Stream);
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Read packet {0}", id.ToString()));
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
                Stream.WriteByte((byte)id);
                p.Write(Stream);
                LastSent = p;
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Sent packet {0}", id.ToString()));
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
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
            foreach (var v in Map)
            {
                if (p.GetType() == v.Value)
                {
                    id = v.Key;
                }
            }
            return id;
        }
        static Packet()
        {
            Map = new Dictionary<PacketId,Type>
            {
                {PacketId.Handshake, typeof(HandshakePacket)},
                {PacketId.RequestMod, typeof(RequestModPacket)},
                {PacketId.FilePart, typeof(FilePartPacket)},
                {PacketId.ModInfo, typeof(ModInfoPacket)},
                {PacketId.Metadata, typeof(MetadataPacket)},
                {PacketId.ModList, typeof(ModListPacket)},
                {PacketId.EncryptionStatus, typeof(EncryptionStatusPacket)},
                {PacketId.NextDownload, typeof(NextDownloadPacket)},
                {PacketId.AllDone, typeof(AllDonePacket)},
                {PacketId.Log, typeof(LogPacket)},
                //{PacketId.ClientUpdate, typeof(ClientUpdatePacket)},
                {PacketId.Disconnect, typeof(DisconnectPacket)},
                //{PacketId.GoodBye, typeof(DisconnectPacket)},
                //{PacketId.Image, typeof(ImagePacket)},
                {PacketId.Connect, typeof(ConnectPacket)}
            };
        }
    }
    #region Packet Classes
    public class HandshakePacket : Packet
    {
        public int Version { get; private set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Version = s.ReadInt();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(PROTOCOL_VERSION);
        }
    }
    public class RequestModPacket : Packet
    {
        public string FileName { get; set; }
        public RequestType Type { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            Type = (RequestType)s.ReadByte();
            FileName = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteByte((byte)Type);
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
    public class ModMetadataPacket : Packet
    {
        public byte[] Part { get; set; }
        public int StartPoint { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            throw new NotImplementedException();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            throw new NotImplementedException();
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
    public class ClientUpdatePacket : Packet
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            int len = s.ReadInt();
            File = s.ReadBytes(len);
            FileName = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(File.Length);
            s.WriteBytes(File);
            s.WriteString(FileName);
        }
    }
    public class MetadataPacket : Packet
    {
        public string[] Data { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            int i = s.ReadInt();
            Data = new string[i];
            for (int j = 0; j < i; j++)
            {
                Data[j] = s.ReadString();
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(Data.Length);
            foreach (string str in Data)
            {
                s.WriteString(str);
            }
        }
    }
    public class AdminPacket : Packet
    {
        public string AdminName { get; set; }
        public string AdminPass { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            AdminName = s.ReadString();
            AdminPass = s.ReadString();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(AdminName);
            s.WriteString(AdminPass);
        }
    }
    public class AdminUploadPacket : Packet
    {
        public string XmlName { get; set; }
        public string ModName { get; set; }
        public byte[] XmlFile { get; set; }
        public byte[] ModFile { get; set; }
        public int XmlLen { get; private set; }
        public int ModLen { get; private set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            XmlName = s.ReadString();
            ModName = s.ReadString();
            XmlLen = s.ReadInt();
            ModLen = s.ReadInt();
            XmlFile = s.ReadBytes(XmlLen);
            ModFile = s.ReadBytes(ModLen);
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteString(XmlName);
            s.WriteString(ModName);
            s.WriteInt(XmlLen);
            s.WriteInt(ModLen);
            s.WriteBytes(XmlFile);
            s.WriteBytes(ModFile);
        }
    }
    public class AdminConfigPacket : Packet
    {
        public int[] ConfigLen { get; set; }
        public string[] ConfigFileNames { get; set; }
        public byte[][] ConfigFiles { get; set; }

        public override void Read(ModUpdaterNetworkStream s)
        {
            ConfigLen = new int[s.ReadInt()];
            for (int i = 0; i < ConfigLen.Length; i++)
            {
                ConfigFileNames[i] = s.ReadString();
                ConfigFiles[i] = s.ReadBytes(ConfigLen[i]);
            }
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            
        }
    }
    public class AdminInfoPacket : Packet
    {
        public override void Read(ModUpdaterNetworkStream s)
        {
            throw new NotImplementedException();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            throw new NotImplementedException();
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
        public byte[] Image { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            int l = s.ReadInt();
            Image = s.ReadBytes(l);
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(Image.Length);
            s.WriteBytes(Image);
        }
    }
    public class ConnectPacket : Packet
    {
        public int ClientID { get; set; }
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public override void Read(ModUpdaterNetworkStream s)
        {
            ClientID = s.ReadInt();
            Address = new IPAddress(s.ReadBytes(4));
            Port = s.ReadInt();
        }

        public override void Write(ModUpdaterNetworkStream s)
        {
            s.WriteInt(ClientID);
            s.WriteBytes(Address.GetAddressBytes());
            s.WriteInt(Port);
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
