//    File:        Connection.cs
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
using System.Net;
using System.Net.Sockets;
using ModUpdater.Net;
using ModUpdater.Admin.Items;
using System.IO;

namespace ModUpdater.Admin.Items
{
    class Connection
    {
        public Server Server { get; private set; }
        public Socket Socket { get; private set; }
        public PacketHandler PacketHandler {get; private set;}
        public ProgressStep CurrentStep
        {
            get { return _step; }
            private set
            {
                ProgressChange(value); 
                _step = value;
            }
        }
        public string ProgressMessage { get; private set; }
        public string SessionID { get; private set; }

        public delegate void ProgressChangeEvent(ProgressStep step);
        public event ProgressChangeEvent ProgressChange = delegate { }; //No need to check if null.

        private ProgressStep _step;
        private string username;
        private string password;

        public Connection(Server s)
        {
            Server = s;
            ProgressChange += new ProgressChangeEvent(OnProgressChange);
        }

        public void Connect()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(Server.IPAddress, Server.Port);
            PacketHandler = new PacketHandler(Socket);
            PacketHandler.Start();
            string reason = "";
            if (!Login(ref reason)) throw new LoginFailedException(reason);
            Packet.Send(new HandshakePacket { Type = HandshakePacket.SessionType.Admin, Username = username }, PacketHandler.Stream);
        }
        public void RegisterUser(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        private bool Login(ref string reason)
        {
            string postdata = "user=" + username + "&password=" + password + "&version=" + int.MaxValue.ToString();
            byte[] post = Encoding.UTF8.GetBytes(postdata);
            WebRequest r = WebRequest.Create("https://login.minecraft.net");
            r.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)r).UserAgent = "Minecraft Mod Updater Login Manager";
            r.Method = "POST";
            r.ContentType = "application/x-www-form-urlencoded";
            r.ContentLength = post.Length;
            Stream s = r.GetRequestStream();
            s.Write(post, 0, post.Length);
            WebResponse wr = r.GetResponse();
            s = wr.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string responce = sr.ReadToEnd();
            reason = responce;
            s.Close();
            sr.Close();
            wr.Close();
            if (!responce.Contains(":")) return false;
            string[] returndata = responce.Split(':');
            SessionID = returndata[3];
            return true;
        }
        private void OnProgressChange(ProgressStep step)
        {
            switch (step)
            {
                case ProgressStep.ConnectingToMinecraft:
                    ProgressMessage = "Connecting to the login server...";
                    break;
                case ProgressStep.LoggingInToMinecraft:
                    ProgressMessage = "Logging into Minecraft...";
                    break;
                case ProgressStep.Connecting:
                    ProgressMessage = "Connecting to the update server...";
                    break;
                case ProgressStep.LoggingIn:
                    ProgressMessage = "Logging in...";
                    break;
                case ProgressStep.DownloadingModInformation:
                    ProgressMessage = "Downloading mod information...";
                    break;
                case ProgressStep.ConnectionFailed:
                    ProgressMessage = "Connection failed.";
                    break;
                case ProgressStep.LoginFailed:
                    ProgressMessage = "Login failed.  Reason: {0}";
                    break;
            }
        }
    }

    public enum ProgressStep
    {
        ConnectingToMinecraft,
        LoggingInToMinecraft,
        Connecting,
        LoggingIn,
        DownloadingModInformation,
        ConnectionFailed,
        LoginFailed,
        Connected
    }
}
