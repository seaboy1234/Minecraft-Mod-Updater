﻿//    File:        SplashScreenForm.cs
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
    public partial class SplashScreenForm : Form
    {
        delegate void StringParameterDelegate(string Text);
        delegate void StringParameterWithStatusDelegate(string Text, TypeOfMessage tom);
        delegate void SplashShowCloseDelegate();
        bool FadeOut = false;
        public bool Loading = true;

        /// <summary>
        /// To ensure splash screen is closed using the API and not by keyboard or any other things
        /// </summary>
        bool CloseSplashScreenFlag = false;

        /// <summary>
        /// Base constructor
        /// </summary>
        public SplashScreenForm()
        {
            InitializeComponent();
            this.lblMessage.BackColor = Color.Transparent;
            lblMessage.Parent = Image;
            this.lblMessage.ForeColor = Color.Green;
            lblTitle.Parent = Image;
            this.lblTitle.BackColor = Color.Transparent;
            this.lblTitle.Text = MinecraftModUpdater.LongAppName;
            this.lblProgress.Parent = Image;
            this.Opacity = 0;
            DownloadPicture.Parent = Image;
            Progress.Show();
        }

        /// <summary>
        /// Displays the splashscreen
        /// </summary>
        public void ShowSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(ShowSplashScreen));
                return;
            }
            this.Show();
            if (SplashScreen.GetScreen() != this) throw new InvalidOperationException("There can only be one splash screen open at once.");
            Application.Run(this);
        }

        /// <summary>
        /// Closes the SplashScreen
        /// </summary>
        public void CloseSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(CloseSplashScreen));
                return;
            }
            CloseSplashScreenFlag = true;
            this.Close();
        }

        /// <summary>
        /// Update text in default green color of success message
        /// </summary>
        /// <param name="Text">Message</param>
        public void UpdateStatusText(string Text)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                Invoke(new StringParameterDelegate(UpdateStatusText), new object[] { Text });
                return;
            }
            // Must be on the UI thread if we've got this far
            lblMessage.ForeColor = Color.Green;
            lblMessage.Text = Text;
        }


        /// <summary>
        /// Update text with message color defined as green/yellow/red/ for success/warning/failure
        /// </summary>
        /// <param name="Text">Message</param>
        /// <param name="tom">Type of Message</param>
        public void UpdateStatusTextWithStatus(string Text, TypeOfMessage tom)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterWithStatusDelegate(UpdateStatusTextWithStatus), new object[] { Text, tom });
                return;
            }
            // Must be on the UI thread if we've got this far
            switch (tom)
            {
                case TypeOfMessage.Error:
                    lblMessage.ForeColor = Color.Red;
                    break;
                case TypeOfMessage.Warning:
                    lblMessage.ForeColor = Color.Yellow;
                    break;
                case TypeOfMessage.Success:
                    lblMessage.ForeColor = Color.Green;
                    break;
            }
            lblMessage.Text = Text;

        }
        public void setDownloadPicture(Image i)
        {
            if (InvokeRequired)
            {
                Invoke(new ModUpdaterDelegate(delegate{ setDownloadPicture(i); }));
            }
            DownloadPicture.Image = i;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Visible) return;
            if (Opacity >= 1.0 && !FadeOut) timer.Stop();
            if (FadeOut)
            {
                this.Opacity -= .05;
                if(this.Opacity <= .0)
                {
                    timer.Stop();
                    Close();
                }
            }
            else
            {
                this.Opacity += .05;
                if (this.Opacity >= 1.0)
                {
                    Loading = false;
                    Visible = true;
                }
            }
        }

        private void SplashScreenForm_Load(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void SplashScreenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseSplashScreenFlag == false)
            {
                //e.Cancel = true;
            }
            else
            {
                if (Opacity >= 1)
                {
                    FadeOut = true;
                    timer.Start();
                    e.Cancel = true;
                    return;
                }
            }
        }
    }
}
