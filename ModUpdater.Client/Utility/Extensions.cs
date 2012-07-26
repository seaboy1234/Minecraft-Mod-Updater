﻿//    File:        Extensions.cs
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

namespace ModUpdater.Client.Utility
{
    public static class ListExtensions
    {
        public static Mod Find(this List<Mod> mods, string identifier)
        {
            foreach (Mod m in mods)
            {
                if (m.Identifier == identifier)
                    return m;
            }
            return null;
        }
        public static Mod FindFromFile(this List<Mod> mods, string file)
        {
            foreach (Mod m in mods.ToArray())
            {
                if (m.File.Replace('/', '\\') == file)
                    return m;
            }
            return null;
        }
        public static bool Contains(this List<Mod> mods, string identifier)
        {
            foreach (Mod m in mods)
            {
                if (m.Identifier == identifier)
                    return true;
            }
            return false;
        }
        public static Mod[] ToArray(this List<Mod> mods, bool optional)
        {
            List<Mod> ms = new List<Mod>();
            foreach (Mod m in mods)
            {
                if (m.Optional)
                {
                    ms.Add(m);
                }
            }
            return ms.ToArray();
        }
    }
}
