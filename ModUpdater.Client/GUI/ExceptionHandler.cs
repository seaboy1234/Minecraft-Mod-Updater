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
using ModUpdater.Client.Utility;

namespace ModUpdater.Client.GUI
{
    public partial class ExceptionHandler : Form
    {
        public static bool ProgramCrashed { get; private set; }
        public static event ModUpdaterDelegate CloseProgram = delegate { };
        public Exception Exception;
        private bool Locked = false;
        private string Report;
        private object ExceptionSender;
        public ExceptionHandler(Exception e, object sender)
        {
            ExceptionSender = sender;
            Exception = e;
            InitializeComponent();
        }

        private void ExceptionHandler_Load(object sender, EventArgs e)
        {
            if (ExceptionSender == null) ExceptionSender = this;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Minecraft Mod Updater has crashed.");
            sb.AppendLine("Please make an error report about this including everything below the line.");
            sb.AppendLine("If you make an error report about this, I will make sure it gets fixed.");
            sb.AppendLine("The application will try to recover from this error, if you wish.");
            sb.AppendLine();
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine("Application: " + MinecraftModUpdater.LongAppName);
            sb.AppendLine("Version: " + Program.Version);
            sb.AppendLine("API Version: " + MinecraftModUpdater.Version);
            sb.AppendLine("OS: " + Environment.OSVersion.ToString());
            sb.AppendLine("Framework Version: " + System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion());
            sb.AppendLine("Exception Sender: " + ExceptionSender.GetType().FullName);
            sb.AppendLine();
            sb.AppendLine(Exception.ToString());
            txtError.Text = sb.ToString();
            Locked = true;
            Report = txtError.Text;
        }
        public static void HandleException(Exception e, object sender)
        {
            if (ProgramCrashed) return;
            ProgramCrashed = true;
            try
            {
                MainForm.Instance.Invoke(new ModUpdaterDelegate(delegate
                {
                    new ExceptionHandler(e, sender).ShowDialog();
                }));
            }
            catch { TaskManager.AddAsyncTask(delegate { new ExceptionHandler(e, sender).ShowDialog(); }, ThreadRole.Important); }
        }
        public static void Init()
        {
            TaskManager.ExceptionRaised += new TaskManagerError(TaskManager_ExceptionRaised);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (!MCModUpdaterExceptionHandler.RegisterExceptionHandler(new ExceptionHandlerLiaison()))
            {
                MessageBox.Show("Error");
            }
        }

        static void TaskManager_ExceptionRaised(Exception e)
        {
            HandleException(e, null);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception, sender);
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((Exception)e.ExceptionObject, sender);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/seaboy1234/Minecraft-Mod-Updater/issues/new");
            using (StreamWriter sw = File.CreateText("ErrorReport.log"))
            {
                
                sw.WriteLine(txtError.Text);
                sw.Close();
            }
            Clipboard.SetText(txtError.Text);
            MessageBox.Show("Error report added to clipboard.");
            Locked = false;
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
            Locked = false;
            Close();
        }

        private void txtError_TextChanged(object sender, EventArgs e)
        {
            if(Locked)
                txtError.Text = Report;
        }

        private void ExceptionHandler_FormClosed(object sender, FormClosedEventArgs e)
        {
            DialogResult r = MessageBox.Show("Would you like to try and recover from this error?  Some progress might be lost.", "Exception Handler", MessageBoxButtons.YesNo);
            if (r == System.Windows.Forms.DialogResult.Yes)
            {
                using (StreamWriter sw = new StreamWriter("recoveryinformation.dat"))
                {
                    sw.WriteLine("status=" + Program.AppStatus);
                    if (Program.AppStatus == Utility.AppStatus.Updating || Program.AppStatus == Utility.AppStatus.Connecting)
                    {
                        sw.WriteLine("server.ip=" + MainForm.Instance.Server.Address);
                        sw.WriteLine("server.port=" + MainForm.Instance.Server.Port);
                        sw.WriteLine("server.name=" + MainForm.Instance.Server.Name);
                    }
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
                Application.Restart();
                return;
            }
            CloseProgram.Invoke();
            Process.GetCurrentProcess().Kill();
        }
    }
}
