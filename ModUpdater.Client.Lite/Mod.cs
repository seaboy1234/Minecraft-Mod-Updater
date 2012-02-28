using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater.Client.Lite
{
    public class Mod
    {
        public string Name { get; private set; }
        public string File { get; set; }
        public string Hash { get; set; }
        public Mod(ModInfoPacket packet)
        {
            Name = packet.ModName;
            File = packet.File;
            Hash = packet.Hash;
        }
        public bool ModIsUpToDate()
        {
            if (Extras.GenerateHash(@".minecraft\" + File) == Hash)
                return true;
            return false;
        }
    }
}
