//    File:        ConnectionForm.cs
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ModUpdater.Net;

namespace ModUpdater.Client.GUI
{
    public partial class ConnectionForm : Form
    {
        public Server ConnectTo;
        public ConnectionForm()
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            InitializeComponent();
            
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Properties.Settings.Default.MinecraftPath))
            {
                Directory.CreateDirectory(Properties.Settings.Default.MinecraftPath);
            }
            Properties.Settings.Default.Server = txtServer.Text;
            Properties.Settings.Default.Port = int.Parse(tempPortTxt.Text);
            if (!CanClose()) return;
            if (Properties.Settings.Default.RememberServer)
            {
                Properties.Settings.Default.Port = ConnectTo.Port;
                Properties.Settings.Default.Server = ConnectTo.Address;
            }
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private bool CanClose()
        {
            while (MainForm.Instance.LocalAddress == null) ;
            if (MainForm.Instance.LocalAddress.ToString() == txtServer.Text) txtServer.Text = "127.0.0.1";
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ConnectionHandler.ConnectTo(s, txtServer.Text, int.Parse(tempPortTxt.Text));
            }
            catch
            {
                MessageBox.Show("Unable to connect to the server.");
                return false;
            }
            ModUpdaterNetworkStream str = new ModUpdaterNetworkStream(s);
            Packet.Send(new HandshakePacket { Type = HandshakePacket.SessionType.ServerList }, str);
            Packet p = Packet.ReadPacket(str); //The server should only return a ServerList, right?
            ServerListPacket sp = null; 
            if (!(p is ServerListPacket)) //But just in case...
            {
                Packet.Send(new DisconnectPacket(), str);
                ConnectTo = new Server { Address = txtServer.Text, Port = int.Parse(tempPortTxt.Text) };
                str.Close();
                s.Disconnect(false);
                return true;
            }
            sp = (ServerListPacket)p;
            List<Server> servers = new List<Server>();
            for (int i = 0; i < sp.Servers.Length; i++)
            {
                Server srv = new Server { Address = sp.Locations[i], Name = sp.Servers[i], Port = sp.Ports[i] };
                servers.Add(srv);
            }
            SelectServerDialog dial = new SelectServerDialog(servers.ToArray());
            dial.ShowDialog();
            if (dial.DialogResult != System.Windows.Forms.DialogResult.OK)
                return false;
            ConnectTo = dial.SelectedServer;
            return true;
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            txtServer.Text = Properties.Settings.Default.Server;
            tempPortTxt.Text = Properties.Settings.Default.Port.ToString();
            KeyDown += new KeyEventHandler(ConnectionForm_KeyDown);
        }

        private void ConnectionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F12)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();
                txtServer.Text = Properties.Settings.Default.Server;
            }
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            new OptionsForm().ShowDialog();
        }
    }
}
