﻿//    File:        Program.cs
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
using System.Threading;
using ModUpdater.Client.GUI;
using ModUpdater.Client.Game;
using Ionic.Zip;
using ModUpdater.Client.Utility;
using System.Reflection;

namespace ModUpdater.Client
{
    static class Program
    {
        public const string Version = "1.3.1";
        public static AppStatus AppStatus;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string s in args)
                {
                    switch (s)
                    {
                        case "-commandline":
                            CommandPromptForm.Open();
                            ProgramOptions.CommandLine = true;
                            break;
                        case "-debug":
                            ProgramOptions.Debug = true;
                            break;
                        case "-reset":
                            Properties.Settings.Default.Reset();
                            Properties.Settings.Default.Save();
                            break;
                    }
                }
            }
            ExceptionHandler.Init();
            AppStatus = Utility.AppStatus.Init;
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            ProgramOptions.Administrator = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            TaskManager.AddAsyncTask(delegate
            {
                //throw new SystemException("Error", new SystemException("Other Error"));
            });
            string name = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(name).Length != 1)
            {
                foreach (Process pr in Process.GetProcessesByName(name))
                {
                    if (pr.MainModule.BaseAddress == Process.GetCurrentProcess().MainModule.BaseAddress)
                        if (pr.Id != Process.GetCurrentProcess().Id)
                            pr.Kill();
                }
            }
            Application.Run(new MainForm());
        }
        public static void StartMinecraft()
        {
            SplashScreen.CloseSplashScreen();
            string javaPath, sessionID, username;
            javaPath = Properties.Settings.Default.JavaPath;
            username = ProgramOptions.Username;
            sessionID = ProgramOptions.SessionID;
            using (FileStream output = File.Open("MCLaunch.class", FileMode.Create))
            {
                using (Stream input = System.Reflection.Assembly.
                        GetCallingAssembly().GetManifestResourceStream("MCLaunch.Launcher"))
                {
                    byte[] buffer = new byte[1024 * 2];
                    int count = 0;
                    while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, count);
                    }
                }
            }
            Process minecraft = new Process();
            ProcessStartInfo mcProcStart = new ProcessStartInfo();

            mcProcStart.FileName = javaPath;
            mcProcStart.Arguments = string.Format(
                "-Xmx1024m -Xms1024m {0} \"{1}\" \"{2}\" \"{3}\"",
                "MCLaunch", Properties.Settings.Default.MinecraftPath, username, sessionID);

            mcProcStart.CreateNoWindow = true;
            mcProcStart.UseShellExecute = false;
            mcProcStart.RedirectStandardOutput = true;
            mcProcStart.RedirectStandardError = true;

            minecraft.StartInfo = mcProcStart;
            minecraft.Start();
            while (!minecraft.StandardOutput.EndOfStream)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, minecraft.StandardOutput.ReadLine().Replace(ProgramOptions.SessionID, "REDACTED"));
            }
            while (!minecraft.StandardError.EndOfStream)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, minecraft.StandardError.ReadLine());
            }
            Thread.Sleep(1000);
            File.Delete("MCLaunch.class");
            if (Properties.Settings.Default.FirstRun)
            {
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }
        }
        public static void UpdateMinecraft()
        {
            if (SplashScreen.GetScreen() == null)
            {
                TaskManager.AddAsyncTask(delegate
                {
                    SplashScreen.ShowSplashScreen();
                });
            }
            Thread.Sleep(100);
            GameUpdater update = new GameUpdater(ProgramOptions.LatestVersion, "minecraft.jar", true);
            SplashScreen.UpdateStatusText("Downloading Minecraft...");
            Thread.Sleep(1000);
            TaskManager.AddAsyncTask(delegate
            {
                SplashScreen.GetScreen().Invoke(new ModUpdaterDelegate(delegate
                {
                    SplashScreen.GetScreen().Progress.Value = 0;
                    SplashScreen.GetScreen().Progress.MaxValue = 100;
                }));
                while (update.Progress != 100)
                {
                    SplashScreen.GetScreen().Invoke(new ModUpdaterDelegate(delegate
                    {
                        SplashScreen.GetScreen().Progress.Value = update.Progress;
                        SplashScreen.UpdateStatusText(update.Status);
                    }));
                    Thread.Sleep(10);
                }
            });
            update.UpdateGame();
            while (update.Progress != 100) ;
            if (!String.IsNullOrEmpty(MainForm.Instance.ClientVersion))
            {
                WebClient cl = new WebClient();
                string client = MainForm.Instance.ClientVersion + ".mcdiff";
                SplashScreen.UpdateStatusText("Downloading Patches...");
                byte[] b = cl.DownloadData("http://mcmodpdater.sourceforge.net/patches/1.3.0/" + client);
                using (FileStream output = File.Open("bspatch.exe", FileMode.Create))
                {
                    using (Stream input = Assembly.GetCallingAssembly().GetManifestResourceStream("ModUpdater.Client.Utility.bspatch.exe"))
                    {
                        byte[] buffer = new byte[1024 * 36];
                        int count = 0;
                        while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, count);
                        }
                    }
                }
                File.Move(Path.Combine(Properties.Settings.Default.MinecraftPath, "bin", "minecraft.jar"), "minecraft.jar");
                File.WriteAllBytes(client, b);
                SplashScreen.UpdateStatusText("If prompted, please press \"Yes\"");
                Process p = Process.Start("bspatch.exe", "minecraft.jar minecraft.jar " + client);
                while (!p.HasExited) { }
                File.Move("minecraft.jar", Path.Combine(Properties.Settings.Default.MinecraftPath, "bin", "minecraft.jar"));
                File.Delete("bspatch.exe");
                File.Delete(client);
            }
            using (ZipFile zf = ZipFile.Read(Path.Combine(Properties.Settings.Default.MinecraftPath, "bin", "minecraft.jar")))
            {
                List<ZipEntry> delete = new List<ZipEntry>();
                foreach (ZipEntry ze in zf)
                {
                    if (ze.FileName.Contains("META-INF"))
                        delete.Add(ze);
                }
                foreach (ZipEntry ze in delete)
                {
                    zf.RemoveEntry(ze);
                }
                zf.Save();
            }
        }

        internal static bool TestJava(string p)
        {
            if (!File.Exists(p)) return false;
            Process java = new Process();
            ProcessStartInfo jStart = new ProcessStartInfo(p, "-version");
            jStart.RedirectStandardOutput = true;
            jStart.RedirectStandardError = true;
            jStart.UseShellExecute = false;
            jStart.CreateNoWindow = true;
            java.StartInfo = jStart;
            java.Start();
            string jOutput = java.StandardError.ReadToEnd();
            if (jOutput.Contains("Java(TM) SE Runtime Environment"))
                return true;
            return false;
        }
        public static void RunOnUIThread(ModUpdaterDelegate method)
        {
            MainForm.Instance.Invoke(method);
        }
    }
}
