using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModUpdater.Net;
using ModUpdater.Admin.Dialog;
using ModUpdater.Utility;

namespace ModUpdater.Admin.GUI
{
    public partial class Watchtower : Form
    {
        MainForm MainForm = MainForm.Instance;
        public Watchtower()
        {
            InitializeComponent();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (lsClients.SelectedItem == null)
            {
                e.Cancel = true;
                return;
            }
            mnuUser.Text = lsClients.SelectedItem.ToString();
        }

        private void kickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "";
            if (InputBox.Dialog("Kick reason", "Kick Client", ref msg) == System.Windows.Forms.DialogResult.OK)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "kick", lsClients.SelectedItem.ToString(), msg } }, MainForm.Connection.PacketHandler.Stream);
            }
        }

        private void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "";
            if (InputBox.Dialog("Message", "Send Message", ref msg) == System.Windows.Forms.DialogResult.OK)
            {
                Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "kick", lsClients.SelectedItem.ToString(), msg } }, MainForm.Connection.PacketHandler.Stream);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            
            if (txtCmd.Text == "") return;
            Packet.Send(new MetadataPacket { SData = new string[] { "watchtower", "command", txtCmd.Text } }, MainForm.Connection.PacketHandler.Stream);
            txtCmd.Text = "";
        }

        private void txtCmd_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                btnSend_Click(null, null);
        }

        public void HandleWatchtower(MetadataPacket p)
        {
            switch (p.SData[1])
            {
                case "message":
                    if (p.SData[2] == "log")
                    {
                        Invoke(new ModUpdaterDelegate(delegate
                        {
                            txtConsole.AppendText(p.SData[3] + "\r\n");
                        }));
                    }
                    else if (p.SData[2] == "message")
                    {
                        MessageBox.Show(p.SData[3], "Server Message");
                    }
                    break;
                case "client_status":
                    if (p.SData[3] == "join")
                    {
                        Invoke(new ModUpdaterDelegate(delegate
                        {
                            lsClients.Items.Add(p.SData[2]);
                        }));
                    }
                    else if (p.SData[3] == "leave")
                    {
                        Invoke(new ModUpdaterDelegate(delegate
                        {
                            lsClients.Items.Remove(p.SData[2]);
                        }));
                    }
                    break;
            }
        }
    }
}
