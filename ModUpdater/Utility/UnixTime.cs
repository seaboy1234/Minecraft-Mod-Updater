using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater.Utility
{
    public class UnixTime
    {
        private static readonly DateTime startOfUnixTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private long unixTime;

        public UnixTime() : this(DateTime.Now) { }
        public UnixTime(DateTime t)
        {
            unixTime = (t - startOfUnixTime).Milliseconds;
        }

        public override string ToString()
        {
            return unixTime.ToString();
        }
    }
}
