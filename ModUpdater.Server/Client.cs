using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ModUpdater.Server
{
    class Client
    {
        public IPEndPoint IPAddress { get; private set; }
        public Server Server {get; private set;}
        public int ClientID { get; private set; }
        public PacketHandler PacketHandler { get { return ph; } }
        private PacketHandler ph;
        private PacketHandler ph2;
        public Client(Socket s, Server sv)
        {
            ph = new PacketHandler(s);
            ph.Handshake += new PacketEvent<HandshakePacket>(RegisterClient);
            ph.RequestMod += new PacketEvent<RequestModPacket>(RetreveMod);
            ph.Log += new PacketEvent<LogPacket>(HandleLog);
            ph.Disconnect += new PacketEvent<Packet>(HandleDisconnect);
            Server = sv;
            IPAddress = (IPEndPoint)s.RemoteEndPoint;
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
                    Packet.Send(new ModInfoPacket { Author = mod.Author, File = mod.ModFile, ModName = mod.ModName, Hash = Extras.GenerateHash(mod.ModFile) }, ph.Stream);
                    break;
                case RequestModPacket.RequestType.Download:
                    byte[] file = File.ReadAllBytes(mod.ModFile);
                    Packet.Send(new NextDownloadPacket { ModName = mod.ModName, FileName = mod.ModFile, Length = file.Length, PostDownloadCLI = mod.PostDownloadCLI }, ph.Stream);
                    List<List<byte>> abyte = new List<List<byte>>();
                    int k = 0;
                    for (int i = 0; i < file.Length; i+= 1024)
                    {
                        abyte.Add(new List<byte>());
                        for (int j = i; j < i + 1024; j++)
                        {
                            if(file.Length > j)
                                abyte[k].Add(file[j]);
                        }
                        k++;
                    }
                    Packet.Send(new ChunkSizePacket { Size = abyte.Count }, ph.Stream);
                    int l = 0;
                    for (int h = 0; h < abyte.Count; h++)
                    {
                        Packet.Send(new FilePartPacket { Part = abyte[h].ToArray(), Index = l }, ph.Stream);
                        l += abyte[h].Count;
                    }
                    Packet.Send(new AllDonePacket { File = mod.ModFile }, ph.Stream);
                    break;
                case RequestModPacket.RequestType.Config:
                    SendConfig(mod);
                    break;
            }
        }

        private void SendConfig(Mod mod)
        {
            //List<byte[]> contents = new List<byte[]>();
            //for(int i = 0; i < mod.ModConfigs.Length; i++)
            //{
            //    contents.Add(File.ReadAllBytes(mod.ModConfigs[i]));
            //}
            
        }

        internal void RegisterClient(HandshakePacket p)
        {
            ClientID = Server.Clients.Count;
            if (Packet.PROTOCOL_VERSION != p.Version)
            {
                Packet.Send(new ClientUpdatePacket { File = File.ReadAllBytes("Client\\ModUpdater.dll")}, ph.Stream);
                Packet.Send(new ClientUpdatePacket { File = File.ReadAllBytes("Client\\MinecraftModUpdater.exe") }, ph.Stream);
                ph.Stop();
                return;
            }
            Console.WriteLine("Client {0} connected. ({1})", ClientID, IPAddress.Address);
            Packet.Send(new EncryptionStatusPacket { Encrypt = true, EncryptionIV = ph.Stream.IV, EncryptionKey = ph.Stream.Key }, ph.Stream);
            ph.Stream.Encrypted = true;
            string[] mods = new string[Server.Mods.Count];
            for (int i = 0; i < Server.Mods.Count; i++)
            {
                mods[i] = Server.Mods[i].ModFile;
            }
            Packet.Send(new ConnectPacket { Address = Server.Address, ClientID = ClientID, Port = 4714 }, ph.Stream);
            Packet.Send(new ModListPacket { Mods = mods}, ph.Stream);
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

        public bool Connected { get { return ph.Stream.Disposed; } }
    }
}
