using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ModUpdater.Net;

namespace ModUpdater.Admin
{
    class Connection
    {
        public IPAddress IPAddress { get; set; }
        public Socket Socket { get; set; }
        public PacketHandler PacketHandler {get; set;}
        public ProgressStep CurrentStep { get; set; }
        public string ProgressMessage { get; set; }

        public delegate void ProgressChangeEvent(ProgressStep step);
        public event ProgressChangeEvent ProgressChange = delegate { }; //No need to check if null.


        public void OnProgressChange(ProgressStep step)
        {
            switch (step)
            {
                case ProgressStep.ConnectingToMinecraft:
                    ProgressMessage = "Connecting to the login server...";
                    break;
                case ProgressStep.LoggingInToMinecraft:
                    ProgressMessage = "Logging into Minecraft...";
                    break;
                case ProgressStep.Connecting:
                    ProgressMessage = "Connecting to the update server...";
                    break;
                case ProgressStep.LoggingIn:
                    ProgressMessage = "Logging in...";
                    break;
                case ProgressStep.DownloadingModInformation:
                    ProgressMessage = "Downloading mod information...";
                    break;
                case ProgressStep.ConnectionFailed:
                    ProgressMessage = "Connection failed.";
                    break;
                case ProgressStep.LoginFailed:
                    ProgressMessage = "Login failed.";
                    break;
            }
        }

        public enum ProgressStep
        {
            ConnectingToMinecraft,
            LoggingInToMinecraft,
            Connecting,
            LoggingIn,
            DownloadingModInformation,
            ConnectionFailed,
            LoginFailed
        }
    }
}
