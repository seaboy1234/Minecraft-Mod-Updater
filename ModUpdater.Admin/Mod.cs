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

namespace ModUpdater.Server
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
         *     <ConfigFiles>
         *         <File>mods\config\filename.txt</File>
         *         <File>config\filename.txt</File>
         *         <File>ModName\config\filename.txt</File>
         *         <File>filename.txt</File>
         *     </ConfigFiles>
         * </Mod>
         */
        public string ModName { get; private set; }
        public string Author { get; private set; }
        public string[] ModConfigs { get; private set; }
        public string ModFile { get; private set; }

        private XmlDocument modFile;
        public Mod(string ConfigFile)
        {
            FileStream fs = new FileStream(ConfigFile, FileMode.Open);
            modFile = new XmlDocument();
            modFile.Load(fs);
            XmlNodeList nodes = modFile.GetElementsByTagName("Mod");
            ModName = nodes[0].ChildNodes[0].InnerText;
            Author = nodes[0].ChildNodes[1].InnerText;
            ModFile = nodes[0].ChildNodes[2].InnerText;
            ModConfigs = new string[nodes[0].ChildNodes[3].ChildNodes.Count];
            int i = 0;
            foreach (XmlNode n in nodes[0].ChildNodes[3].ChildNodes)
            {
                ModConfigs[i] = n.InnerText;
                i++;
            }
        }
    }
}
