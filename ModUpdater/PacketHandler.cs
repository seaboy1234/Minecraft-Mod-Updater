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
