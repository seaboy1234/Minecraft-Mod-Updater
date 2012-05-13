//    File:        CommandPromptForm.cs
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

namespace ModUpdater.Client.GUI
{
    public partial class CommandPromptForm : Form
    {
        private delegate void LogMessage(Logger.Level level, string message);
        public CommandPromptForm()
        {
            InitializeComponent();
        }

        internal static void Open()
        {
            TaskManager.AddAsyncTask(delegate
            {
                new CommandPromptForm().ShowDialog();
            });
        }

        private void CommandPromptForm_Load(object sender, EventArgs e)
        {
            foreach(string s in MinecraftModUpdater.Logger.GetMessages())
            {
                textBox1.AppendText(s + Environment.NewLine);
            }
            MinecraftModUpdater.Logger.LogEvent += new Logger.LogEventDelegate(Logger_LogEvent);
        }

        void Logger_LogEvent(Logger.Level level, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new LogMessage(Logger_LogEvent), level, message);
                return;
            }
            if ((int)level >= 0 || ProgramOptions.Debug)
            {
                textBox1.AppendText(string.Format("[{0}] {1}{2}", level, message, Environment.NewLine));
            }
        }
        private void CommandPromptForm_Resize(object sender, EventArgs e)
        {
            textBox1.Size = new Size(Size.Width - 15, Size.Height - 36);
        }

        private void CommandPromptForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MinecraftModUpdater.Logger.LogEvent -= new Logger.LogEventDelegate(Logger_LogEvent);
        }
    }
}
