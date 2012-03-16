using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using ModUpdater.Utility;
using ModUpdater.Net;

namespace ModUpdater.Server.Master
{
    class Server
    {
        public List<Slave> Servers { get; private set; }
        private TcpListener listen;
        private bool Online;
        public Server()
        {
            Servers = new List<Slave>();
            Online = false;
            listen = new TcpListener(IPAddress.Any, Properties.Settings.Default.Port);
        }
        public void Start() 
        {
            Online = true;
            listen.Start();
        }
        public void Stop()
        {
            Online = false;
            listen.Stop();
        }
        public void Listen()
        {
            while (Online)
            {
                Socket s = listen.AcceptSocket();
                TaskManager.AddAsyncTask(delegate
                {
                    PacketHandler ph = new PacketHandler(s);
                    ph.Handshake += new PacketEvent<HandshakePacket>(delegate(HandshakePacket p)
                        {
                            if (p.Type == HandshakePacket.SessionType.Server)
                            {
                                Servers.Add(new Slave(p, ph));
                                return;
                            }
                            string[] srvs = new string[Servers.Count];
                            string[] addrs = new string[Servers.Count];
                            int[] ports = new int[Servers.Count];
                            for(int i = 0; i < Servers.Count; i++)
                            {
                                srvs[i] = Servers[i].Name;
                                addrs[i] = Servers[i].Address.ToString();
                                ports[i] = Servers[i].Port;
                            }
                            Packet.Send(new ServerListPacket { Servers = srvs, Locations = addrs, Ports = ports }, ph.Stream);
                            ph.Stop();
                            ph = null;
                        });
                });
            }
        }
    }
}
