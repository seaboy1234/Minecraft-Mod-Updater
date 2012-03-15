//    File:        PacketHandler.cs
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
using System.Threading;

namespace ModUpdater
{
    public class PacketHandler
    {
        public ModUpdaterNetworkStream Stream { get; set; }
        protected Socket sck;
        private bool IgnoreNext = false;
        private Thread NetworkThread;
        /*Events*/
        public event PacketEvent<FilePartPacket> FilePart;
        public event PacketEvent<HandshakePacket> Handshake;
        public event PacketEvent<MetadataPacket> Metadata;
        public event PacketEvent<ModInfoPacket> ModInfo;
        public event PacketEvent<ModListPacket> ModList;
        public event PacketEvent<RequestModPacket> RequestMod;
        public event PacketEvent<NextDownloadPacket> NextDownload;
        public event PacketEvent<LogPacket> Log;
        public event PacketEvent<AllDonePacket> AllDone;
        public event PacketEvent<Packet> Disconnect;
        public event PacketEvent<ConnectPacket> Connect;
        public event PacketEvent<ImagePacket> Image;
        /*End Events*/

        public PacketHandler(Socket s)
        {
            sck = s;
            NetworkThread = new Thread(new ThreadStart(delegate { while (sck.Connected) { Recv(); } }));
        }
        /// <summary>
        /// Handles receving of packets.  This method should never be called from outside of this class.
        /// </summary>
        private void Recv()
        {
            if (IgnoreNext)
            {
                return;
            }
            PacketId id;
            Packet p;
            try
            {
                p = Packet.ReadPacket(Stream);
                id = Packet.GetPacketId(p);
                switch (id)
                {
                    case PacketId.EncryptionStatus:
                        EncryptionStatusPacket pa = (EncryptionStatusPacket)p;
                        Stream.Encrypted = pa.Encrypt;
                        Stream.IV = pa.EncryptionIV;
                        Stream.Key = pa.EncryptionKey;
                        break;
                    case PacketId.Handshake:
                        Handshake.Invoke((HandshakePacket)p);
                        break;
                    case PacketId.Metadata:
                        Metadata.Invoke((MetadataPacket)p);
                        break;
                    case PacketId.ModInfo:
                        ModInfo.Invoke((ModInfoPacket)p);
                        break;
                    case PacketId.ModList:
                        ModList.Invoke((ModListPacket)p);
                        break;
                    case PacketId.RequestMod:
                        RequestMod.Invoke((RequestModPacket)p);
                        break;
                    case PacketId.AllDone:
                        AllDone.Invoke((AllDonePacket)p);
                        break;
                    case PacketId.NextDownload:
                        NextDownload.Invoke((NextDownloadPacket)p);
                        break;
                    case PacketId.Log:
                        Log.Invoke((LogPacket)p);
                        break;
                    case PacketId.FilePart:
                        FilePart.Invoke((FilePartPacket)p);
                        break;
                    case PacketId.Disconnect:
                        if (Disconnect != null)
                            Disconnect.Invoke(p);
                        break;
                    case PacketId.Connect:
                        if (Connect != null)
                            Connect.Invoke((ConnectPacket)p);
                        break;
                    case PacketId.Image:
                        if (Image != null)
                            Image.Invoke((ImagePacket)p);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MinecraftModUpdater.Logger.Log(e);
            }
        }
        /// <summary>
        /// Starts the networking thread and begins handling packets.
        /// </summary>
        public void Start()
        {
            Stream = new ModUpdaterNetworkStream(sck);
            NetworkThread.Name = "Network";
            NetworkThread.IsBackground = true;
            NetworkThread.Start();
        }
        /// <summary>
        /// Stops the networking thread and stops handling packets.
        /// </summary>
        public void Stop()
        {
            Stream.Dispose();
            sck.Disconnect(false);
            NetworkThread.Abort();
        }
    }
}
