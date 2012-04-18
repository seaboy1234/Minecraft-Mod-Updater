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

namespace ModUpdater.Admin
{
    public class Mod
    {
        /*
         * Standard Mod File Format
         * <?xml version="1.0"?>
         * <Mod>
         *     <Name>ModName</Name>
         *     <Author>Author</Author>
         *     <File>mods\modfile.zip</File>
         *     <PostDownload>
         *         <Action>somecommand</Action>
         *         <Action>someothercommand</Action>
         *     </PostDownload>
         *     <Whitelist>
         *         <Username Value="somename" />
         *         <Username Value="namesome" />
         *     </Whitelist>
         *     <Blacklist>
         *         <Username Value="somename" />
         *         <Username Value="namesome" />
         *     </Blacklist>
         * </Mod>
         */
        public string Name { get; set; }
        public string Author { get; set; }
        public string File { get; set; }
        public string[] PostDownloadCLI { get; set; }
        public List<string> WhitelistedUsers { get; set; }
        public List<string> BlacklistedUsers { get; set; }
        public Mod()
        {
            Name = "";
            Author = "";
            File = "";
            PostDownloadCLI = new string[0];
            WhitelistedUsers = new List<string>();
            BlacklistedUsers = new List<string>();
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
