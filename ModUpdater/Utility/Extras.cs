﻿//    File:        Extras.cs
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
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace ModUpdater.Utility
{
    public static class Extras
    {
        /// <summary>
        /// Generates a hash based on a given string
        /// </summary>
        /// <param name="filePathAndName">The file to generate a hash for.</param>
        /// <returns>A hash based off the file given.</returns>
        public static string GenerateHashFromString(string input)
        {
            string hashText = "";
            string hexValue = "";

            byte[] stringData = Encoding.UTF8.GetBytes(input);
            byte[] hashData = SHA1.Create().ComputeHash(stringData); // SHA1 or MD5

            foreach (byte b in hashData)
            {
                hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                hashText += (hexValue.Length == 1 ? "0" : "") + hexValue;
            }
            return hashText;
        }
        /// <summary>
        /// Generates a hash based on the file given.
        /// </summary>
        /// <param name="filePathAndName">The file to generate a hash for.</param>
        /// <returns>A hash based off the file given.</returns>
        public static string GenerateHash(string filePathAndName)
        {
            string hashText = "";
            string hexValue = "";

            byte[] fileData = File.ReadAllBytes(filePathAndName);
            byte[] hashData = SHA1.Create().ComputeHash(fileData); // SHA1 or MD5

            foreach (byte b in hashData)
            {
                hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                hashText += (hexValue.Length == 1 ? "0" : "") + hexValue;
            }
            return hashText;
        }
        /// <summary>
        /// Generates a hash based on the file given.
        /// </summary>
        /// <param name="fileContents">The raw bytes of the file</param>
        /// <returns>A hash based off the file given.</returns>
        public static string GenerateHash(byte[] fileContents)
        {
            string hashText = "";
            string hexValue = "";

            byte[] fileData = fileContents;
            byte[] hashData = SHA1.Create().ComputeHash(fileData); // SHA1 or MD5

            foreach (byte b in hashData)
            {
                hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                hashText += (hexValue.Length == 1 ? "0" : "") + hexValue;
            }
            return hashText;
        }
        /// <summary>
        /// Generates an image from the raw bytes given.
        /// </summary>
        /// <param name="bytes">The raw bytes to transform into an image</param>
        /// <returns>An image generated from the raw bytes given.</returns>
        public static Image ImageFromBytes(byte[] bytes)
        {
            Image picture = null;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                picture = Image.FromStream(ms);
                ms.Dispose();
            }
            return picture;
        }
        /// <summary>
        /// Gets the bytes of an image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] BytesFromImage(Image image)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
                ms.Dispose();
            }
            return bytes;
        }
        public static byte[] ImageBytesFromFile(string path)
        {
            Image i = Image.FromFile(path);
            return BytesFromImage(i);
        }
        public static bool CheckForUpdate(string key, string value, out string newVer, out bool api)
        {
            bool requireUpdate = false;
            api = false;
            WebClient c = new WebClient();
            string raw = c.DownloadString("https://raw.github.com/seaboy1234/Minecraft-Mod-Updater/" + MinecraftModUpdater.Branch + "/version.txt");
            c.Dispose();
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            foreach (string s in raw.Split('\\'))
            {
                string _key = s.Split('=')[0];
                string _value = s.Split('=')[1];
                pairs.Add(_key, _value);
            }
            newVer = "";
            newVer = pairs[key];
            if (pairs[key] != value)
            {
                requireUpdate = true;
            }
            else if (pairs["core"] != MinecraftModUpdater.Version)
            {
                api = true;
                requireUpdate = true;
                newVer = pairs["core"];
            }
            return requireUpdate;
        }
        public static IPAddress GetAddressFromHostname(string hostname)
        {
            IPAddress[] ip = Dns.GetHostAddresses(hostname);
            return ip.FirstOrDefault();
        }
    }
}
