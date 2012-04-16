﻿//    File:        Config.cs
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
using System.Xml;
using System.IO;

namespace ModUpdater.Server
{
    class Config
    {
        /// <summary>
        /// The port to run the server on.
        /// Default: 4713
        /// </summary>
        public static int Port { get; set; }
        /// <summary>
        /// The max clients able to be connected to the server.
        /// </summary>
        public static int MaxClients { get; set; }
        /// <summary>
        /// The path the the mods and xml folders.
        /// </summary>
        public static string ModsPath { get; set; }
        /// <summary>
        /// The name of this update server.
        /// </summary>
        public static string ServerName { get; set; }
        /// <summary>
        /// The IP Address and port of the master server.
        /// </summary>
        public static string MasterServer { get; set; }
        private static XmlDocument config = new XmlDocument();
        public static void Load()
        {
            config = new XmlDocument();
            try
            {
                if (File.Exists("Config.xml"))
                    config.Load("Config.xml");
            }
            catch { }
            XmlNodeList nodes = config.SelectNodes("/Config");
            XmlNode n = nodes[0];
            try
            {
                Port = int.Parse(n["Port"].InnerText);
            }
            catch { Port = 4713; }
            try
            {
                MaxClients = int.Parse(n["MaxClients"].InnerText);
            }
            catch { MaxClients = 15; }
            try
            {
                ModsPath = n["ModsPath"].InnerText;
            }
            catch
            { ModsPath = "."; }
            try
            {
                ServerName = n["ServerName"].InnerText;
            }
            catch
            { ServerName = "Minecraft Mod Updater v" + MinecraftModUpdater.Version; }
            try
            {
                MasterServer = n["MasterServer"].InnerText;
            }
            catch
            { MasterServer = ""; }
            try
            {
                config.Save("Config.xml");
            }
            catch { } //XML file is not valid.
        }
        public static void Save()
        {
            bool rootexists = true;
            XmlNodeList nodes = config.SelectNodes("/Config");
            XmlNode n = nodes[0];
            if (n == null) rootexists = false;
            XmlElement port;
            XmlElement maxclients;
            XmlElement modspath;
            XmlElement servername;
            XmlElement master;
            if (!rootexists)
            {
                XmlDeclaration dec = config.CreateXmlDeclaration("1.0", null, null);
                config.AppendChild(dec);
                XmlElement root = config.CreateElement("Config");
                config.AppendChild(root);
                port = config.CreateElement("Port");
                root.AppendChild(port);
                maxclients = config.CreateElement("MaxClients");
                root.AppendChild(maxclients);
                modspath = config.CreateElement("ModsPath");
                root.AppendChild(modspath);
                servername = config.CreateElement("ServerName");
                root.AppendChild(servername);
                master = config.CreateElement("MasterServer");
            }
            else
            {
                port = n["Port"];
                maxclients = n["MaxClients"];
                modspath = n["ModsPath"];
                servername = n["ServerName"];
                master = n["MasterServer"];
            }
            port.InnerText = Port.ToString();
            maxclients.InnerText = MaxClients.ToString();
            modspath.InnerText = ModsPath;
            servername.InnerText = ServerName;
            master.InnerText = MasterServer;
            try
            {
                config.Save("Config.xml");
            }
            catch
            {
                File.WriteAllText("Config.xml", config.OuterXml);
            }
        }
    }
}
