using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater
{
    /// <summary>
    /// Handles debug messages from the core functions.
    /// </summary>
    public static class DebugMessageHandler
    {
        public delegate void DebugMessage(string message);
        public static event DebugMessage DebugMessages;
        public static event DebugMessage CommandLineMessages;
        /// <summary>
        /// Assert a message to be sent to a registered debug handler.
        /// </summary>
        /// <param name="message">The message to send.</param>
        internal static void Assert(string message)
        {
            if (DebugMessages != null)
                DebugMessages.Invoke(message);
        }
        internal static void AssertCl(string message)
        {
            if (CommandLineMessages != null)
                CommandLineMessages.Invoke(message);
        }
    }
}
