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
            { ModsPath = ""; }
            try
            {
                ServerName = n["ServerName"].InnerText;
            }
            catch
            { ServerName = "Minecraft Mod Updater v" + MinecraftModUpdater.Version; }
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
            }
            else
            {
                port = n["Port"];
                maxclients = n["MaxClients"];
                modspath = n["ModsPath"];
                servername = n["ServerName"];
            }
            port.InnerText = Port.ToString();
            maxclients.InnerText = MaxClients.ToString();
            modspath.InnerText = ModsPath;
            servername.InnerText = ServerName;
            File.WriteAllText("Config.xml", config.OuterXml);
        }
    }
}
