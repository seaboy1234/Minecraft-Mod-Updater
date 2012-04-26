//    File:        ExceptionHandler.cs
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using ModUpdater.Utility;

namespace ModUpdater.Client.GUI
{
    public partial class ExceptionHandler : Form
    {
        public static bool ProgramCrashed { get; private set; }
        public Exception Exception;
        private bool Locked = false;
        private string Report;
        public ExceptionHandler(Exception e)
        {
            Exception = e;
            InitializeComponent();
        }

        private void ExceptionHandler_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Minecraft Mod Updater has crashed.");
            sb.AppendLine("Please make an error report about this including everything below the line.");
            sb.AppendLine("If you make an error report about this, I will make sure it gets fixed.");
            sb.AppendLine();
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine("Application: " + MinecraftModUpdater.LongAppName);
            sb.AppendLine("Version: " + Program.Version);
            sb.AppendLine("API Version: " + MinecraftModUpdater.Version);
            sb.AppendLine("OS: " + Environment.OSVersion.ToString());
            sb.AppendLine("Framework Version: " + System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion());
            sb.AppendLine();
            sb.AppendLine(Exception.ToString());
            txtError.Text = sb.ToString();
            Locked = true;
            Report = txtError.Text;
        }
        public static void HandleException(Exception e)
        {
            if (ProgramCrashed) return;
            ProgramCrashed = true;
            try
            {
                MainForm.Instance.Invoke(new MainForm.Void(delegate
                {
                    new ExceptionHandler(e).ShowDialog();
                }));
            }
            catch { new ExceptionHandler(e).ShowDialog(); }
        }
        public static void Init()
        {
            TaskManager.ExceptionRaised += new TaskManager.Error(HandleException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((Exception)e.ExceptionObject);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/seaboy1234/Minecraft-Mod-Updater/issues/new");
            using (StreamWriter sw = File.CreateText("ErrorReport.log"))
            {
                
                sw.WriteLine(txtError.Text);
                sw.Close();
            }
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = File.CreateText("ErrorReport.log"))
            {
                sw.WriteLine(txtError.Text);
                sw.Close();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtError_TextChanged(object sender, EventArgs e)
        {
            if(Locked)
                txtError.Text = Report;
        }

        private void ExceptionHandler_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
