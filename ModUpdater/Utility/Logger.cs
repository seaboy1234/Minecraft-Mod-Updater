//    File:        Logger.cs
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

namespace ModUpdater.Utility
{
    public class Logger
    {
        List<string> StringLogs = new List<string>();
        List<Level> LevelLogs = new List<Level>();
        public enum Level
        {
            Info,
            Warning,
            Error
        }
        public void Log(Level l, string s)
        {
            StringLogs.Add(s);
            LevelLogs.Add(l);
            DebugMessageHandler.AssertCl("["+l.ToString().ToUpper()+"] " + s);
        }
        public void Log(Exception e)
        {
            Log(Level.Error, e.ToString());
        }
        public string[] GetMessages()
        {
            List<string> strs = new List<string>();
            for(int i = 0; i < StringLogs.Count; i++)
            {
                strs.Add(string.Format("[{0}] {1}", LevelLogs[i].ToString().ToUpper(), StringLogs[i]));
            }
            return strs.ToArray();
        }
    }
}
