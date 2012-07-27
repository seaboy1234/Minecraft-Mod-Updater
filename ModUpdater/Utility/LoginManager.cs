//    File:        LoginManager.cs
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
using System.IO;

namespace ModUpdater.Utility
{
    /// <summary>
    /// Manages client logins.
    /// </summary>
    public class LoginManager
    {
        /// <summary>
        /// The client's username.  This may be changed after calling Login(string, out string[]).
        /// </summary>
        public string Username { get; private set; }
        /// <summary>
        /// The client's session id.  Unassigned until calling Login(string, out string[]).
        /// </summary>
        public string SessionID { get; private set; }
        /// <summary>
        /// Whether to continue registering with Mojang's auth servers.
        /// </summary>
        public bool SessionTick { get; set; }

        /// <summary>
        /// Initializes a new instance of the LoginManager class.
        /// </summary>
        /// <param name="username">The login name of the client.</param>
        /// <param name="sessionTick">Whether to keep the client's session registered with the session server.</param>
        public LoginManager(string username, bool sessionTick = true)
        {
            Username = username;
            SessionTick = sessionTick;
        }

        /// <summary>
        /// Registers the client with Mojang's auth server.
        /// </summary>
        /// <param name="password">The client's password.</param>
        /// <param name="output">An empty string array with a size of 4.</param>
        /// <returns>Whether authentication was successful or unsucessful, a populated output of size 4.</returns>
        /// <remarks>If authentication is unsucessful, the first string in the output array will be the reason stating why.</remarks>
        public bool Login(string password, out string[] output)
        {
            //Prep data
            output = new string[4];
            string postdata = "user=" + Username + "&password=" + password + "&version=" + int.MaxValue.ToString();
            byte[] post = Encoding.UTF8.GetBytes(postdata);
            //Init streams.
            WebRequest r = WebRequest.Create("https://login.minecraft.net");
            r.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)r).UserAgent = "Minecraft Mod Updater Login Manager";
            r.Method = "POST";
            r.ContentType = "application/x-www-form-urlencoded";
            r.ContentLength = post.Length;
            Stream s = r.GetRequestStream();
            //Write the data.
            s.Write(post, 0, post.Length);
            //Get a responce.
            WebResponse wr = r.GetResponse();
            s = wr.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string responce = sr.ReadToEnd();
            //Close streams.
            s.Close();
            sr.Close();
            wr.Close();
            //Parse the responce.
            output[0] = responce;
            if (!responce.Contains(":")) return false;
            string[] returndata = responce.Split(':'); //We can assume that this is safe because of the above line.
            SessionID = returndata[3];
            Username = returndata[2]; // Make sure to set the username.
            output = returndata;
            if (SessionTick) TaskManager.AddAsyncTask(TickSession, ThreadRole.Delayed, 300 * 1000);
            return true;
        }

        /// <summary>
        /// Keeps the client's session active.  This method is self-resetting.
        /// </summary>
        private void TickSession()
        {
            if (!SessionTick) return;
            string postdata = "user=" + Username + "&session=" + SessionID;
            byte[] post = Encoding.UTF8.GetBytes(postdata);
            //Init streams.
            WebRequest r = WebRequest.Create("https://login.minecraft.net/session");
            r.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)r).UserAgent = "Minecraft Mod Updater Login Manager";
            r.Method = "POST";
            r.ContentType = "application/x-www-form-urlencoded";
            r.ContentLength = post.Length;
            Stream s = r.GetRequestStream();
            //Write the data.
            s.Write(post, 0, post.Length);
            s.Close();
            TaskManager.AddAsyncTask(TickSession, ThreadRole.Delayed, 300 * 1000);
        }
    }
}
