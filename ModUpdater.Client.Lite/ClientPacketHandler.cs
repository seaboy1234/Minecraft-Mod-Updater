using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ModUpdater.Client
{
    class ClientPacketHandler : PacketHandler
    {
        public ClientPacketHandler(Socket s) : base(s) { }
    }
}
