﻿using System;
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
        public string ModName { get; private set; }
        public string Author { get; private set; }
        public string ModFile { get; private set; }
        public string[] PostDownloadCLI { get; private set; }
        public List<string> WhitelistedUsers { get; private set; }
        public List<string> BlacklistedUsers { get; private set; }
        private XmlDocument modFile;
        public Mod(string ConfigFile)
        {
            modFile = new XmlDocument();
            modFile.Load(ConfigFile);
            XmlNodeList nodes = modFile.SelectNodes("/Mod");
            XmlNode n = nodes[0];
            try
            {
                ModName = n["Name"].InnerText;
            }
            catch { }
            try
            {
                Author = n["Author"].InnerText;
            }
            catch { }
            try
            {
                ModFile = n["File"].InnerText;
            }
            catch { }
            try
            {
                XmlNode cfg = n["ConfigFiles"];
                if (n["ConfigFiles"] != null)
                {
                    n.RemoveChild(cfg);
                }
            }
            catch { }
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
            catch { }
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
            catch { }
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
            catch { }
            modFile.Save(ConfigFile);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + ModName);
            sb.AppendLine("Author: " + Author);
            sb.AppendLine("File Path: " + ModFile);
            sb.AppendLine("Post Download Actions: ");
            foreach (string s in PostDownloadCLI)
            {
                sb.AppendLine("    " + s);
            }
            return sb.ToString();
        }
    }
}
