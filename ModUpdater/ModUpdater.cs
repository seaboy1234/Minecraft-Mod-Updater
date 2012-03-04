using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater
{
    /// <summary>
    /// Class contains basic information about the app
    /// </summary>
    public static class MinecraftModUpdater
    {
        /// <summary>
        /// The shortname.
        /// </summary>
        public const string ShortAppName = "Mod Updater";
        /// <summary>
        /// The name.
        /// </summary>
        public const string LongAppName = "Minecraft Mod Updater";
        /// <summary>
        /// The Mod Updater Version.
        /// </summary>
        public const string Version = "1.2.0_dev";
        /// <summary>
        /// The Logger.
        /// </summary>
        public static readonly Logger Logger = new Logger();
    }
}
