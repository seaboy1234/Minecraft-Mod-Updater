using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ModUpdater.Client.Lite
{
    public class Client
    {
        private List<Mod> mods;
        private ClientPacketHandler ph;
        private Socket s;
        private ModUpdaterNetworkStream m;
        public bool Start()
        {
            mods = new List<Mod>();
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ph = new ClientPacketHandler(s);
            TaskManager.AddSyncTask(delegate { s.Connect(IPAddress.Parse(Properties.Settings.Default.Server), 4713); });
            if (!s.Connected)
            {
                return false;
            }
            ph.Start();
            m = ph.Stream;
            ph.ModList += new PacketEvent<ModListPacket>(ph_ModList);
            ph.ModInfo += new PacketEvent<ModInfoPacket>(ph_ModInfo);
            return true;
        }
        public Mod GetMod(string name)
        {
            foreach (Mod m in mods)
            {
                if (m.Name.ToLower() == name.ToLower())
                    return m;
            }
            return null;
        }
        public Mod[] GetMods()
        {
            return mods.ToArray();
        }
        void ph_ModInfo(ModInfoPacket p)
        {
            mods.Add(new Mod(p));
        }

        void ph_ModList(ModListPacket p)
        {
            foreach (string s in p.Mods)
            {
                Packet.Send(new RequestModPacket { Type = RequestModPacket.RequestType.Info, FileName = s }, m);
            }
        }
    }
}
