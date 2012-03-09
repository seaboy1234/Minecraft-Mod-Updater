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
                    Packet.Send(new ModInfoPacket { Author = mod.Author, File = mod.ModFile, ModName = mod.ModName, Hash = Extras.GenerateHash(Config.ModsPath + "\\" + mod.ModFile) }, ph.Stream);
                    break;
                case RequestModPacket.RequestType.Download:
                    byte[] file = File.ReadAllBytes(Config.ModsPath + "\\" + mod.ModFile);
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
                    Packet.Send(new NextDownloadPacket { ModName = mod.ModName, FileName = mod.ModFile, Length = file.Length, PostDownloadCLI = mod.PostDownloadCLI, ChunkSize = abyte.Count }, ph.Stream);
                    int l = 0;
                    for (int h = 0; h < abyte.Count; h++)
                    {
                        Packet.Send(new FilePartPacket { Part = abyte[h].ToArray(), Index = l }, ph.Stream);
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
            ClientID = Server.Clients.Count;
            if (Packet.PROTOCOL_VERSION != p.Version)
            {
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
            Packet.Send(new MetadataPacket { SData = new string[] { "server_name", Config.ServerName }, FData = new float[] { 24.0f } }, ph.Stream);
            if (Server.BackgroundImage != null)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "splash_display", "Downloading Assets..." } }, ph.Stream);
                Packet.Send(new ImagePacket { Type = ImagePacket.ImageType.Background, ShowOn = "", Image = Extras.BytesFromImage(Server.BackgroundImage) }, ph.Stream);
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
