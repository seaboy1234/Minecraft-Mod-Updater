//    File:        LoggerForm.cs
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
using ModUpdater.Utility;

namespace ModUpdater.Admin.GUI
{
    public partial class LoggerForm : Form
    {
        public LoggerForm()
        {
            InitializeComponent();
        }

        private void LoggerForm_Resize(object sender, EventArgs e)
        {
            textBox1.Size = this.ClientSize;
        }

        private void LoggerForm_Load(object sender, EventArgs e)
        {
            textBox1.Size = this.ClientSize;
            MinecraftModUpdater.Logger.LogEvent += new LogEventDelegate(Logger_LogEvent);
        }

        void Logger_LogEvent(Logger.Level level, string message)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                Invoke(new LogEventDelegate(Logger_LogEvent), level, message);
                return;
            }
            if ((int)level >= 0)
            {
                try
                {
                    textBox1.AppendText(string.Format("[{0}] {1}{2}", level, message, Environment.NewLine));
                }
                catch (InvalidOperationException) { }
                catch (Exception e) { Program.HandleException(e); }
            }
        }
    }
}
