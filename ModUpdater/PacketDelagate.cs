using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater
{
    public delegate void PacketEvent<T>(T p) where T : Packet;
}
