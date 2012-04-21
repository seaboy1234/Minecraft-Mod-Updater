using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater.Net
{
    class MalformedPacketException : Exception
    {
        public MalformedPacketException(string message) : base(message) { }
    }
}
