using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Drawing;

namespace ModUpdater.Server
{
    class Server
    {
        public List<Mod> Mods { get; private set; }
        public List<Client> Clients { get; private set; }
        public TcpListener TcpServer { get; private set; }
        public TcpListener TcpServer2 { get; private set; }
        public IPAddress Address { get; private set; }
        public Dictionary<Mod, Image> ModImages { get; private set; }
        bool Online { get; set; }
        public Server()
        {
            Config.Load();
            Mods = new List<Mod>();
            Clients = new List<Client>();
            TcpServer = new TcpListener(IPAddress.Any, Config.Port);
            foreach (string s in Directory.GetFiles("xml"))
            {
                Mods.Add(new Mod(s));
            }
            foreach (Mod m in Mods)
            {
                //Console.WriteLine(m.ToString());
            }
            Console.WriteLine("Registered {0} mods", Mods.Count);
        }
        private void SelfUpdate()
        {
            if (Directory.Exists(@"clientmods\xml")) Directory.Move(@"clientmods\xml", "xml");
            if (Directory.Exists(@"clientmods\config")) Directory.Move(@"clientmods\config", "config");
            if (Directory.Exists("clientmods")) Directory.Move("clientmods", "mods");
            if (Directory.Exists("mods")) Directory.Move("mods", Config.ModsPath + "\\mods");
            if (Directory.Exists("xml")) Directory.Move("xml", Config.ModsPath + "\\xml");
            if (!Directory.Exists(Config.ModsPath + "\\mods")) Directory.CreateDirectory("mods");
            if (!Directory.Exists(Config.ModsPath + "\\xml")) Directory.CreateDirectory("xml");
        }
        public void Start()
        {
            Address = IPAddress.Loopback;
            try
            {
                string direction = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        direction = stream.ReadToEnd();
                    }
                }
                int first = direction.IndexOf("Address: ") + 9;
                int last = direction.LastIndexOf("</body>");
                direction = direction.Substring(first, last - first);
                Address = IPAddress.Parse(direction);
            }
            catch (Exception e) { MinecraftModUpdater.Logger.Log(e); }
            Console.WriteLine("Server IP Address is: " + Address.ToString());
            TcpServer.Start();
            Online = true;
            TaskManager.AddDelayedAsyncTask(delegate { SimpleConsoleImputHandler(); }, 3000);
            Receve();
        }
        public void Dispose()
        {
            TcpServer.Stop();
            Online = false;
            Config.Save();
        }
        public void Receve()
        {
            while (Online)
            {
                try
                {
                    Socket s = TcpServer.AcceptSocket();
                    TaskManager.AddAsyncTask(delegate
                    {
                        AcceptClient(s);
                    });
                }
                catch { }
            }
        }
        public void AcceptClient(Socket s)
        {
            if (Clients.Count >= Config.MaxClients)
            {
                s.Disconnect(false);
                return;
            }
            Client c = new Client(s, this);
            Clients.Add(c);
            c.StartListening();
            Thread.Sleep(1000);
            while (s.Connected) Thread.Sleep(100);
            c.Dispose();
            Clients.Remove(c);
        }
        public void SimpleConsoleImputHandler()
        {
            Console.WriteLine("Simple Console Input Handler is online and ready.  \r\nEnter \"help\" for a list of commands.");
            while (Online)
            {
                string input = Console.ReadLine();
                if (!Online) break;
                switch (input)
                {
                    case "connected":
                        Console.WriteLine("There are {0} connected clients.", Clients.Count);
                        if (Clients.Count > 0)
                        {
                            Console.WriteLine("Connected Clients:");
                            foreach (Client c in Clients)
                            {
                                Console.WriteLine(c.ToString());
                            }
                        }
                        break;
                    case "exit":
                    case "stop":
                        foreach (Client c in Clients)
                        {
                            Packet.Send(new MetadataPacket { SData = new string[] { "shutdown", "The Server is shutting down." } }, c.PacketHandler.Stream);
                        }
                        if (Clients.Count > 0) Console.WriteLine("Waiting for {0} clients to exit.", Clients.Count);
                        while (Clients.Count > 0) ;
                        Dispose();
                        break;
                    case "help":
                    case "?":
                    default:
                        Console.WriteLine("exit, stop - Safely stops the update server after all clients exit.");
                        Console.WriteLine("connected - Shows a list of connected clients.");
                        break;
                }
            }
        }
    }
}
