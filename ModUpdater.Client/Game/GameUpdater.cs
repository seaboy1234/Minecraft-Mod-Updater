//    File:        GameUpdater.cs
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
using System.Windows.Forms;
using ModUpdater.Client.Properties;
using System.Net;
using System.Security.Cryptography;
using Ionic.Zip;
using ModUpdater.Utility;
using ModUpdater.Client.GUI;

namespace ModUpdater.Client.Game
{
    class GameUpdater
    {
        string mainGameUrl;
        string latestVersion;
        Uri[] uriList;
        bool forceUpdate;
        bool shouldUpdate;
        int totalDownloadSize;
        int currentDownloadSize;
        EnumState state;
        public string Status;
        public int Progress;
        private EnumState State
        {
            get { return state; }
            set
            {
                state = value;
                switch (value)
                {
                    case EnumState.INIT:
                        Status = "Initializing loader";
                        break;
                    case EnumState.DETERMINING_PACKAGES:
                        Status = "Determining packages to load";
                        break;
                    case EnumState.CHECKING_CACHE:
                        Status = "Checking cache for existing files";
                        break;
                    case EnumState.DOWNLOADING:
                        Status = "Downloading packages";
                        break;
                    case EnumState.EXTRACTING_PACKAGES:
                        Status = "Extracting downloaded packages";
                        break;
                    case EnumState.UPDATING_CLASSPATH:
                        Status = "Updating classpath";
                        break;
                    case EnumState.SWITCHING_APPLET:
                        Status = "Switching applet";
                        break;
                    case EnumState.INITIALIZE_REAL_APPLET:
                        Status = "Initializing real applet";
                        break;
                    case EnumState.START_REAL_APPLET:
                        Status = "Starting real applet";
                        break;
                    case EnumState.DONE:
                        Status = "Done loading";
                        break;
                }
            }
        }

        public GameUpdater(string latestVersion,
                           string mainGameUrl,
                           bool forceUpdate = false)
        {
            this.latestVersion = latestVersion;
            this.mainGameUrl = mainGameUrl;
            this.forceUpdate = forceUpdate;
        }

        public void UpdateGame()
        {
            try
            {
                State = EnumState.CHECKING_CACHE;
                Progress = 5;

                // Get a list of URLs to download from
                LoadJarURLs();

                // Create the bin directory if it doesn't exist
                if (!Directory.Exists(".minecraft/bin"))
                    Directory.CreateDirectory(".minecraft/bin");

                string binDir = ".minecraft/bin";
                if (this.latestVersion != null)
                {
                    string versionFile = Path.Combine(binDir, "version");
                    bool cacheAvailable = false;

                    if (!forceUpdate && File.Exists(versionFile) &&
                        (latestVersion.Equals("-1") ||
                     latestVersion.Equals(File.ReadAllText(versionFile))))
                    {
                        cacheAvailable = true;
                        Progress = 90;
                    }

                    if ((forceUpdate) || (!cacheAvailable))
                    {
                        shouldUpdate = true;
                        if (this.shouldUpdate)
                        {
                            WriteVersionFile(versionFile, latestVersion);

                            try
                            {
                                DownloadJars();
                            }
                            catch (WebException e)
                            {
                                throw new Exception("An error occurred when downloading packages.", e);
                            }
                            ExtractNatives();
                            Progress = 100;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                ExceptionHandler.HandleException(new Exception("An error occurred when trying to download Minecraft.", e));
                                             
                return;
            }
        }
        private void LoadJarURLs()
        {
            State = EnumState.DETERMINING_PACKAGES;
            string[] jarList = new string[]
			{ 
				this.mainGameUrl, "lwjgl_util.jar", "jinput.jar", "lwjgl.jar"
			};

            this.uriList = new Uri[jarList.Length + 1];
            Uri mojangBaseUri = new Uri(Resources.MCDownload);

            for (int i = 0; i < jarList.Length; i++)
            {
                this.uriList[i] = new Uri(mojangBaseUri, jarList[i]);
            }

            string nativeJar = string.Empty;

            if (OS.Windows)
                nativeJar = "windows_natives.jar";
            else if (OS.Linux)
                nativeJar = "linux_natives.jar";
            else if (OS.MacOSX)
                nativeJar = "macosx_natives.jar";
            else
            {
                MessageBox.Show("Your operating system is not supported.");
                return;
            }

            this.uriList[this.uriList.Length - 1] = new Uri(mojangBaseUri, nativeJar);
        }
        private void DownloadJars()
        {
            Dictionary<string, string> md5s = new Dictionary<string, string>();
            if (File.Exists(Path.Combine(".minecraft/bin/", "md5s")))
            {
                foreach (string s in File.ReadAllLines(Path.Combine(".minecraft/bin/", "md5s")))
                {
                    string key, value;
                    key = s.Split('=')[0].Trim();
                    value = s.Split('=')[1].Trim();
                    md5s.Add(key, value);
                }
            }
            State = EnumState.DOWNLOADING;

            int[] fileSizes = new int[this.uriList.Length];
            bool[] skip = new bool[this.uriList.Length];

            // Get the headers and decide what files to skip downloading
            for (int i = 0; i < uriList.Length; i++)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Getting header " + uriList[i].ToString());

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriList[i]);
                request.Timeout = 1000 * 15; // Set a 15 second timeout
                request.Method = "HEAD";

                string etagOnDisk = null;
                if (md5s.ContainsKey(Path.GetFileName((uriList[i].LocalPath))))
                    etagOnDisk = md5s[Path.GetFileName((uriList[i].LocalPath))];

                if (!forceUpdate && !string.IsNullOrEmpty(etagOnDisk))
                    request.Headers[HttpRequestHeader.IfNoneMatch] = etagOnDisk;

                using (HttpWebResponse response = ((HttpWebResponse)request.GetResponse()))
                {
                    int code = (int)response.StatusCode;
                    if (code == 300)
                        skip[i] = true;

                    fileSizes[i] = (int)response.ContentLength;
                    this.totalDownloadSize += fileSizes[i];
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Got response: " + code + " and file size of " +
                                      fileSizes[i] + " bytes");
                }
            }

            int initialPercentage = Progress;

            byte[] buffer = new byte[1024 * 10];
            for (int i = 0; i < this.uriList.Length; i++)
            {
                if (skip[i])
                {
                    Progress = (initialPercentage + fileSizes[i] *
                                (100 - initialPercentage) / this.totalDownloadSize);
                }
                else
                {
                    string currentFile = Path.GetFileName((uriList[i].LocalPath));

                    if (currentFile == "minecraft.jar" && File.Exists("mcbackup.jar"))
                        File.Delete("mcbackup.jar");

                    md5s.Remove(currentFile);
                    List<string> lines = new List<string>();
                    foreach (var v in md5s)
                    {
                        lines.Add(v.Key + "=" + v.Value);
                    }
                    File.WriteAllLines(Path.Combine(".minecraft/bin/", "md5s"), lines.ToArray());

                    int failedAttempts = 0;
                    const int MAX_FAILS = 3;
                    bool downloadFile = true;

                    // Download the files
                    while (downloadFile)
                    {
                        downloadFile = false;

                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriList[i]);
                        request.Headers[HttpRequestHeader.CacheControl] = "no-cache";

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        string etag = "";
                        // If downloading from Mojang, use ETag.
                        if (uriList[i].ToString().StartsWith(Resources.MCDownload))
                        {
                            etag = response.Headers[HttpResponseHeader.ETag];
                            etag = etag.TrimEnd('"').TrimStart('"');
                        }
                        // If downloading from dropbox, ignore MD5s
                        else
                        {
                            // TODO add a way to verify integrity of files downloaded from dropbox
                        }

                        Stream dlStream = response.GetResponseStream();
                        using (FileStream fos =
                            new FileStream(Path.Combine(".minecraft/bin", currentFile), FileMode.Create))
                        {
                            int fileSize = 0;

                            using (MD5 digest = MD5.Create())
                            {
                                digest.Initialize();
                                int readSize;
                                while ((readSize = dlStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    //							MinecraftModUpdater.Logger.Log(Logger.Level.Info"Read " + readSize + " bytes");
                                    fos.Write(buffer, 0, readSize);

                                    this.currentDownloadSize += readSize;
                                    fileSize += readSize;

                                    digest.TransformBlock(buffer, 0, readSize, null, 0);

                                    //							Progress = fileSize / fileSizes[i];

                                    Progress = (initialPercentage + this.currentDownloadSize *
                                                (100 - initialPercentage) / this.totalDownloadSize);
                                }
                                digest.TransformFinalBlock(new byte[] { }, 0, 0);

                                dlStream.Close();

                                string md5 = HexEncode(digest.Hash).Trim();
                                etag = etag.Trim();

                                bool md5Matches = true;
                                if (!string.IsNullOrEmpty(etag) && !string.IsNullOrEmpty(md5))
                                {
                                    // This is temporarily disabled since dropbox doesn't use MD5s as etags
                                    md5Matches = md5.Equals(etag);
                                    //							MinecraftModUpdater.Logger.Log(Logger.Level.Infomd5 + "\n" + etag + "\n");
                                }

                                if (md5Matches && fileSize == fileSizes[i] || fileSizes[i] <= 0)
                                {
                                    md5s[(currentFile.Contains("natives") ?
                                          currentFile : currentFile)] = etag;
                                    lines.Clear();
                                    foreach (var v in md5s)
                                    {
                                        lines.Add(v.Key + "=" + v.Value);
                                    }
                                    File.WriteAllLines(Path.Combine(".minecraft/bin/", "md5s"), lines.ToArray());
                                }
                                else
                                {
                                    failedAttempts++;
                                    if (failedAttempts < MAX_FAILS)
                                    {
                                        downloadFile = true;
                                        this.currentDownloadSize -= fileSize;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to download " + currentFile +
                                                       " MD5 sums did not match.");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void ExtractNatives()
        {
            State = EnumState.EXTRACTING_PACKAGES;

            string nativesJar =
                Path.Combine(".minecraft/bin", Path.GetFileName((uriList[uriList.Length - 1].LocalPath)));
            string nativesDir = Path.Combine(".minecraft/bin", "natives");

            if (!Directory.Exists(nativesDir))
                Directory.CreateDirectory(nativesDir);

            using (ZipFile zf = new ZipFile(nativesJar))
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, string.Format("Extracting natives from {0} to {1}", nativesJar, nativesDir));
                ExtractRecursive(zf, nativesDir);
            }

            if (Directory.Exists(Path.Combine(nativesDir, "META-INF")))
                Directory.Delete(Path.Combine(nativesDir, "META-INF"), true);

            File.Delete(nativesJar);
        }

        private void ExtractRecursive(ZipFile zf, string dest, string pathinzip = "/")
        {
            foreach (ZipEntry entry in zf)
            {
                if (entry.FileName.Contains("META-INF"))
                    continue;

                if (entry.IsDirectory)
                {
                    string dir = Path.Combine(dest, entry.FileName);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    ExtractRecursive(zf, dir, string.Format("{0}/{1}", pathinzip, entry.FileName));
                }
                else
                {
                    string destFile = Path.Combine(dest, entry.FileName);
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Extracting to " + destFile);

                    if (!Directory.Exists(dest))
                        Directory.CreateDirectory(dest);

                    using (FileStream destStream = File.Open(destFile, FileMode.Create))
                    {
                        entry.Extract(destStream);
                    }
                }
            }
        }
        public static string ReadVersionFile(string vfile)
        {
            if (!File.Exists(vfile))
                return null;
            string data = "";
            using (Stream stream = File.OpenRead(vfile))
            {
                BinaryReader binRead = new BinaryReader(stream);
                data = binRead.ReadString();
                return data;
            }
        }

        public static void WriteVersionFile(string vfile, string version)
        {
            using (Stream stream = File.Open(vfile, FileMode.Create))
            {
                BinaryWriter binWrite = new BinaryWriter(stream);
                binWrite.Write(version);
                binWrite.Flush();
            }
        }

        public static string HexEncode(byte[] rawbytes)
        {
            char[] HexLowerChars = new[] 
		    { 
			    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
		    };
            int length = rawbytes.Length;
            char[] chArray = new char[2 * length];
            int index = 0;
            int num3 = 0;

            while (index < length)
            {
                chArray[num3++] = HexLowerChars[rawbytes[index] >> 4];
                chArray[num3++] = HexLowerChars[rawbytes[index] & 15];
                index++;
            }
            return new string(chArray);
        }
        public enum EnumState
        {
            INIT, // 1
            DETERMINING_PACKAGES, // 2
            CHECKING_CACHE, // 3
            DOWNLOADING, // 4
            EXTRACTING_PACKAGES, // 5
            UPDATING_CLASSPATH, // 6
            SWITCHING_APPLET, // 7
            INITIALIZE_REAL_APPLET, // 8
            START_REAL_APPLET, // 9
            DONE, // 10
        }
    }
    public class OS
    {
        public static bool Windows
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32Windows:
                        return true;

                    default:
                        return false;
                }
            }
        }
        public static bool Linux
        {
            get
            {
                if (MacOSX)
                    return false;
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 128);
            }
        }
        public static bool MacOSX
        {
            get
            {
                if (Directory.GetParent(Environment.CurrentDirectory).Parent.Name.EndsWith(".app"))
                {
                    return true;
                }

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        return true;

                    default:
                        return false;
                }
            }
        }
    }
}
