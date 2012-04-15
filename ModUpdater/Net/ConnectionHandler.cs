using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using ModUpdater.Utility;

namespace ModUpdater.Net
{
    public class ConnectionHandler
    {
        public static Socket ConnectTo(Socket s, string address, int port)
        {
            IPAddress host = null;
            try
            {
                host = IPAddress.Parse(address);
            }
            catch //We know what happened, the address is a domain.
            {
                host = Extras.GetAddressFromHostname(address);
            }
            s.Connect(host, port);
            return s;
        }
    }
}
