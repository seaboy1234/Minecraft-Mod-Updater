//    File:        Program.cs
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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Net;
using ModUpdater.Utility;

namespace ModUpdater.Client
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern int AllocConsole();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (File.Exists("ModUpdater.Client.exe")) File.Delete("ModUpdater.Client.exe");
            if (args.Length > 0)
            {
                foreach (string s in args)
                {
                    switch (s)
                    {
                        case "-commandline":
                            AllocConsole();
                            ProgramOptions.CommandLine = true;
                            break;
                        case "-debug":
                            ProgramOptions.Debug = true;
                            break;
                        case "-updatemode":
                            ProcessStartInfo i = new ProcessStartInfo();
                            i.Arguments = "Security_Unlock_Code_Delta_Beta_7";
                            TaskManager.AddAsyncTask(delegate { Process.Start(i); });
                            Application.Exit();
                            break;
                    }
                }
            }
            ExceptionHandler.Init();
            Console.WriteLine("Started.");
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            ProgramOptions.Administrator = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        public static void StartMinecraft()
        {
            using (StreamWriter log = File.CreateText("log.txt"))
            {
                if (!File.Exists("Minecraft.exe"))
                {
                    Console.WriteLine("Downloading Minecraft.exe...");
                    new WebClient().DownloadFile("https://s3.amazonaws.com/MinecraftDownload/launcher/Minecraft.exe", "Minecraft.exe");
                }
                Console.WriteLine("Starting Minecraft");
                using (StreamWriter sw = File.AppendText("start.bat"))
                {
                    sw.WriteLine(@"SET APPDATA=%cd%");
                    sw.WriteLine(@"Minecraft.exe {0} {1}", Properties.Settings.Default.Username, Properties.Settings.Default.Password);
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
                ProcessStartInfo info = new ProcessStartInfo("cmd", "/c start.bat");
                info.RedirectStandardOutput = true;
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = info;
                proc.Start();
                log.WriteLine(proc.StandardOutput.ReadToEnd());
                while (File.Exists("Minecraft.exe"))
                {
                    try
                    {
                        File.Delete("Minecraft.exe");
                        break;
                    }
                    catch { }
                    System.Threading.Thread.Sleep(10000);
                }
                while (File.Exists("start.bat"))
                {
                    try
                    {
                        File.Delete("start.bat");
                        break;
                    }
                    catch { }
                    System.Threading.Thread.Sleep(10000);
                }
            }
        }

    }
}
