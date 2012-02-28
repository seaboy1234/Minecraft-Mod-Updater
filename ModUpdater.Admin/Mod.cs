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
