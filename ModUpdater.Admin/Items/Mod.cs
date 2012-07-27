//    File:        Mod.cs
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
using System.IO;
using System.Xml;
using System.Threading;
using ModUpdater.Utility;

namespace ModUpdater.Admin.Items
{
    public class Mod
    {
        public long Size { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string File { get; set; }
        public string Hash { get; set; }
        public string Identifier { get; set; }
        public List<string> PostDownloadCLI { get; set; }
        public List<string> WhitelistedUsers { get; set; }
        public List<string> BlacklistedUsers { get; set; }
        public List<string> RequiredMods { get; set; }
        public bool NeedsUpdate { get; set; }
        public byte[] Contents { get; set; }
        public bool Optional { get; set; }
        public Mod()
        {
            Name = "";
            Author = "";
            File = "";
            Hash = "";
            PostDownloadCLI = new List<string>();
            WhitelistedUsers = new List<string>();
            BlacklistedUsers = new List<string>();
            RequiredMods = new List<string>();
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
