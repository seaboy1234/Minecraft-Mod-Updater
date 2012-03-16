using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ModUpdater.Net;

namespace ModUpdater.Server.Master
{
    class Slave
    {
        public string Name { get; private set; }
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        public PacketHandler PacketHandler { get; private set; }
        public Slave(HandshakePacket p, PacketHandler ph)
        {
            Name = p.Name;
            Address = IPAddress.Parse(p.Address);
            Port = p.Port;
            PacketHandler = ph;
        }
    }
}
