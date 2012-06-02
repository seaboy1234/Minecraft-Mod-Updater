using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModUpdater.Net;

namespace ModUpdater.Utility
{
    public delegate void ModUpdaterDelegate<T>(T args);
    public delegate void LogEventDelegate(Logger.Level level, string message);
    public delegate void Task();
    public delegate void TaskManagerError(Exception e);
    public delegate void PacketEvent(Packet p);
    public delegate void DebugMessage(string message);

}
