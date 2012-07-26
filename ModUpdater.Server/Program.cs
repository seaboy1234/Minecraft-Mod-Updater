//    File:        Program.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModUpdater.Utility;

namespace ModUpdater.Server
{
    class Program
    {
        public const string Version = "1.3.0_1";
        public static string ConfigPath = "Config.xml";
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string s in args)
                {
                    if(s.StartsWith("-config:") || s.StartsWith("-c:"))
                            ConfigPath = GetValueOfParm(s);
                }
            }
            Server server = new Server();
            try
            {
                server.Start();
                MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Server stopped.");
            }
            catch (Exception e)
            {
                MinecraftModUpdater.Logger.Log(e);
                MinecraftModUpdater.Logger.Log(Logger.Level.Info,"Press any key to close.");
                Console.ReadKey();
            }
        }
        static string GetValueOfParm(string parm)
        {
            string[] sa = parm.Split(':');
            if (sa.Length > 1)
            {
                string s = "";
                for (int i = 1; i < sa.Length; i++)
                {
                    s += sa[i];
                    s += ":";
                }
                s = s.Remove(s.Length - 1);
                return s;
            }
            return "";
        }
    }
}
