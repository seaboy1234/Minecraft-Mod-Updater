using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ModUpdater.Utility;

namespace ModUpdater.Server
{
    class Upgrade
    {
        public static void From12x()
        {
            Directory.CreateDirectory(Config.ModsPath + "/assets/mod");
            foreach (string s in Directory.GetFiles(Config.ModsPath + "/ModAssets"))
            {
                File.Move(s, Config.ModsPath + "/assets/mod/" + Path.GetFileName(s));
            }

            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Updated server to " + Program.Version);
        }
    }
}
