using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModUpdater.Client
{
    public static class Debug
    {
        static Debug()
        {
            DebugMessageHandler.CommandLineMessages += new DebugMessageHandler.DebugMessage(DebugMessageHandler_CommandLineMessages);
            DebugMessageHandler.DebugMessages +=new DebugMessageHandler.DebugMessage(Assert);
        }

        static void DebugMessageHandler_CommandLineMessages(string message)
        {
            Console.WriteLine(message);
        }
        public static void Assert(string message)
        {
            if (ProgramOptions.Debug)
            {
                MessageBox.Show(message, "DEBUG MESSAGE");
            }
        }
        public static void Assert(Exception e)
        {
            Assert(e.ToString());
        }
    }
}
