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
using ModUpdater.Utility;

namespace ModUpdater.Net
{
    public class PacketHandler
    {
        public ModUpdaterNetworkStream Stream { get; set; }
        public bool Online
        {
            get
            {
                return online;
            }
            private set
            {
                if(value == false)
                    Disconnect.Invoke(this, EventArgs.Empty);
                online = value;
            }
        }
        public event EventHandler Disconnect = delegate { };
        private bool online;
        protected Socket sck;
        private Thread NetworkThread;
        private Thread ConnectedThread;
        private Dictionary<PacketId, PacketEvent> EventHandler;
        private List<Packet> PacketBacklog;
        public PacketHandler(Socket s)
        {
            sck = s;
            NetworkThread = new Thread(new ThreadStart(delegate
            {
                while (sck.Connected) { Recv(); }
                Stop();
            }));
            ConnectedThread = new Thread(new ThreadStart(delegate
            {
                while (sck.Connected && !Stream.Disposed) Thread.Sleep(3000);
                Stop();
            }));
            EventHandler = new Dictionary<PacketId, PacketEvent>();
            PacketBacklog = new List<Packet>();
        }
        /// <summary>
        /// Handles receving of packets.  This method should never be called from outside of this class.
        /// </summary>
        private void Recv()
        {
            PacketId id;
            Packet p;
            try
            {
                p = Packet.ReadPacket(Stream);
                id = Packet.GetPacketId(p);
                PacketBacklog.Add(p);
                if (id == PacketId.EncryptionStatus)
                {
                    EncryptionStatusPacket pa = p as EncryptionStatusPacket;
                    Stream.IV = pa.EncryptionIV;
                    Stream.Key = pa.EncryptionKey;
                    Stream.Encrypted = pa.Encrypt;
                    return;
                }
                foreach (var ph in EventHandler)
                {
                    if (ph.Key == id)
                    {
                        ph.Value.Invoke(p);
                    }
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
            online = true;
            Stream = new ModUpdaterNetworkStream(sck);
            NetworkThread.Name = "Network";
            NetworkThread.IsBackground = true;
            NetworkThread.Start();
            ConnectedThread.IsBackground = true;
            ConnectedThread.Name = "Connection Thread";
            ConnectedThread.Start();
        }
        /// <summary>
        /// Stops the networking thread and stops handling packets.
        /// </summary>
        public void Stop()
        {
            if (!online) return;
            Online = false;
            Stream.Dispose();
            sck.Disconnect(false);
            NetworkThread.Abort();
        }
        /// <summary>
        /// Registers a packet handler.  This is NOT needed for an EncryptionStatus packet.
        /// </summary>
        /// <param name="id">The packet id.</param>
        /// <param name="handler">The handler for the packet.</param>
        public void RegisterPacketHandler(PacketId id, PacketEvent handler)
        {
            try
            {
                EventHandler.Add(id, handler);
            }
            catch (Exception e) { throw e; }
        }
        /// <summary>
        /// Un-registers the packet handler for a packet.
        /// </summary>
        /// <param name="id">The packet id to un-register.</param>
        public void RemovePacketHandler(PacketId id)
        {
            try
            {
                EventHandler.Remove(id);
            }
            catch (Exception e) { throw e; }
        }
        /// <summary>
        /// Gets a backlog of all recived packets.
        /// </summary>
        /// <returns>Array of packets containing every packet recived during this session.</returns>
        public Packet[] GetBacklog()
        {
            return PacketBacklog.ToArray();
        }
    }
}
