﻿//    File:        Mod.cs
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
using ModUpdater.Net;

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
         *     <Description>
         *         Some words about the mod to explain it to users
         *     </Description>
         * </Mod>
         */
        public string ModName { get; internal set; }
        public string Author { get; internal set; }
        public string ModFile { get; internal set; }
        public string Identifier { get; internal set; }
        public string Description { get; internal set; }
        public string Hash { get; set; }
        public string ConfigFile { get; internal set; }
        public string[] PostDownloadCLI { get; internal set; }
        public bool Optional { get; internal set; }
        public long FileSize { get; internal set; }
        public List<string> WhitelistedUsers { get; internal set; }
        public List<string> BlacklistedUsers { get; internal set; }
        public List<Mod> RequiredMods { get; internal set; }

        private List<List<byte>> FileParts;
        private XmlDocument modFile;
        public Mod()
        {
            ModName = "Unnamed";
            Author = "No Author Given.";
            ModFile = "";
            PostDownloadCLI = new string[0];
            BlacklistedUsers = new List<string>();
            WhitelistedUsers = new List<string>();
            FileSize = 0;
            Description = "No description given.";
            FileParts = new List<List<byte>>();
        }
        public Mod(string ConfigFile)
        {
            ModName = "Unnamed";
            Author = "No Author Given.";
            ModFile = "";
            PostDownloadCLI = new string[0];
            BlacklistedUsers = new List<string>();
            WhitelistedUsers = new List<string>();
            FileSize = 0;
            Description = "No description given.";

            modFile = new XmlDocument();
            FileParts = new List<List<byte>>();
            modFile.Load(ConfigFile);
            this.ConfigFile = ConfigFile;
            XmlNodeList nodes = modFile.SelectNodes("/Mod");
            XmlNode n = nodes[0];
            try
            {
                ModName = n["Name"].InnerText;
            }
            catch
            {
                n.AppendChild(modFile.CreateElement("Name"));
            }
            try
            {
                Author = n["Author"].InnerText;
            }
            catch { n.AppendChild(modFile.CreateElement("Author")); }
            try
            {
                ModFile = n["File"].InnerText;
            }
            catch { n.AppendChild(modFile.CreateElement("File")); }
            try
            {
                Optional = bool.Parse(n["Optional"].InnerText);
            }
            catch { Optional = false; }
            try
            {
                PostDownloadCLI = new string[0];
                XmlNode node = n["PostDownload"];
                PostDownloadCLI = new string[node.ChildNodes.Count];
                int i = 0;
                foreach (XmlNode action in node)
                {
                    if (action.Name != "Action")
                        continue;
                    PostDownloadCLI[i] = action.InnerText;
                    i++;
                }
            }
            catch { n.AppendChild(modFile.CreateElement("PostDownload")); }
            try
            {
                WhitelistedUsers = new List<string>();
                XmlNode node = n["Whitelist"];
                int i = 0;
                foreach (XmlNode user in node)
                {
                    if (user.Name != "Username")
                        continue;
                    WhitelistedUsers.Add(user.InnerText);
                    i++;
                }
            }
            catch { n.AppendChild(modFile.CreateElement("Whitelist")); }
            try
            {
                BlacklistedUsers = new List<string>();
                XmlNode node = n["Blacklist"];
                int i = 0;
                foreach (XmlNode user in node)
                {
                    if (user.Name != "Username")
                        continue;
                    BlacklistedUsers.Add(user.InnerText);
                    i++;
                }
            }
            catch { n.AppendChild(modFile.CreateElement("Blacklist")); }
            try
            {
                if(!string.IsNullOrEmpty(n["Description"].InnerText))
                    Description = n["Description"].InnerText;
            }
            catch { n.AppendChild(modFile.CreateElement("Description")); }
            try
            {
                Identifier = n["Identifier"].InnerText;
            }
            catch
            {
                n.AppendChild(modFile.CreateElement("Identifier"));
                string unix = Extras.GenerateHashFromString(new UnixTime().ToString());
                string id = "";
                for (int i = 0; i < 8; i++)
                {
                    id += unix[i];
                }
                n["Identifier"].InnerText = "";
            }
            if (ModFile.Contains("minecraft.jar"))
            {
                TaskManager.AddAsyncTask(delegate
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Console.Beep();
                        Thread.Sleep(50);
                    }
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info,"WARNING: Sending minecraft.jar is not allowed under the Minecraft Terms of Use.  Please send jar mods in bin/jarmods.jar.");
                });
            }
            ReadFile();
        }
        internal void LoadRequiredMods(List<Mod> mods)
        {
            RequiredMods = new List<Mod>();
            string[] modids = new string[256];
            XmlNodeList nodes = modFile.SelectNodes("/Mod");
            XmlNode n = nodes[0];
            try
            {
                XmlNode node = n["Requires"];
                modids = new string[node.ChildNodes.Count];
                int i = 0;
                foreach (XmlNode mid in node)
                {
                    if (mid.Name != "Mod")
                        continue;
                    modids[i] = mid.InnerText;
                    i++;
                }
            }
            catch { n.AppendChild(modFile.CreateElement("Requires")); }
            foreach (Mod m in mods)
            {
                if (modids.Contains(m.Identifier))
                    RequiredMods.Add(m);
            }
        }
        internal void SendFileTo(Client c)
        {
            PacketHandler ph = c.PacketHandler;
            Packet.Send(new NextDownloadPacket { Identifier = Identifier, PostDownloadCLI = PostDownloadCLI, ChunkSize = FileParts.Count }, ph.Stream);
            int l = 0;
            for (int h = 0; h < FileParts.Count; h++)
            {
                byte[] b = FileParts[h].ToArray();
                Packet.Send(new FilePartPacket { Part = b, Index = l }, ph.Stream);
                l += FileParts[h].Count;
            }
            Packet.Send(new AllDonePacket { Identifier = Identifier }, ph.Stream);

        }
        internal void ReadFile()
        {
            try
            {
                ReadFile(File.ReadAllBytes(Config.ModsPath + "\\" + ModFile));
            }
            catch(Exception e)
            {
                Console.WriteLine("Unable to read file: {0}", e);
                throw e;
            }
        }
        internal void ReadFile(byte[] b)
        {
            try
            {
                byte[] file = b;
                int k = 0;
                FileSize = file.Length;
                for (int i = 0; i < file.Length; i += 2048)
                {
                    FileParts.Add(new List<byte>());
                    for (int j = i; j < i + 2048; j++)
                    {
                        if (file.Length > j)
                            FileParts[k].Add(file[j]);
                    }
                    k++;
                }
                Hash = Extras.GenerateHash(b);
            }
            catch (FileNotFoundException e)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, "{0} not found.  Please check that the name is correct and that the file exists.", ModFile);
                MinecraftModUpdater.Logger.Log(e);
            }
        }
        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(File.Create(ConfigFile)))
            {
                sw.WriteLine("<Mod>");
                sw.WriteLine(" <Name>{0}</Name>", ModName);
                sw.WriteLine(" <Author>{0}</Author>", Author);
                sw.WriteLine(" <File>{0}</File>", ModFile);
                sw.WriteLine(" <Optional>{0}</Optional>", Optional);
                sw.WriteLine(" <PostDownload>");
                foreach (string s in PostDownloadCLI)
                {
                    sw.WriteLine("  <Action>{0}</Action>", s);
                }
                sw.WriteLine(" </PostDownload>");
                sw.WriteLine(" <Blacklist>");
                foreach (string s in BlacklistedUsers)
                {
                    sw.WriteLine("  <Username>{0}</Username>", s);
                }
                sw.WriteLine(" </Blacklist>");
                sw.WriteLine(" <Whitelist>");
                foreach (string s in WhitelistedUsers)
                {
                    sw.WriteLine("  <Username>{0}</Username>", s);
                }
                sw.WriteLine(" </Whitelist>");
                sw.WriteLine(" <Description>{0}</Description>", Description);
                sw.WriteLine(" <Identifier>{0}</Identifier>", Identifier);
                sw.WriteLine(" <Requires>");
                if (RequiredMods != null)
                {
                    foreach (Mod m in RequiredMods)
                    {
                        sw.WriteLine("  <Mod>" + m.Identifier + "</Mod");
                    }
                }
                sw.WriteLine(" </Requires>");
                sw.WriteLine("</Mod>");
                sw.Close();
            }
        }
        public override string ToString()
        {
            return Identifier + "." + ModName;
        }

    }
}
