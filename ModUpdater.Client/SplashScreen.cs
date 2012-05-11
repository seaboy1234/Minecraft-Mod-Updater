//    File:        SplashScreen.cs
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
using System.Drawing;
using ModUpdater.Client.GUI;

namespace ModUpdater.Client
{
    /// <summary>
    /// Defined types of messages: Success/Warning/Error.
    /// </summary>
    public enum TypeOfMessage
    {
        Success,
        Warning,
        Error,
    }
    /// <summary>
    /// Initiate instance of SplashScreen
    /// </summary>
    public static class SplashScreen
    {
        static SplashScreenForm sf = null;
        public static Image BackgroundImage = null;
        /// <summary>
        /// Displays the splashscreen
        /// </summary>
        public static void ShowSplashScreen()
        {
            if (sf == null)
            {
                sf = new SplashScreenForm();
                if (BackgroundImage != null)
                {
                    sf.Image.Image = BackgroundImage;
                    sf.Image.Height = 400;
                    sf.Image.Width = 640;
                }
                sf.ShowSplashScreen();
            }
        }

        /// <summary>
        /// Closes the SplashScreen
        /// </summary>
        public static void CloseSplashScreen()
        {
            if (sf != null)
            {
                sf.CloseSplashScreen();
                //sf = null;
            }
        }

        /// <summary>
        /// Update text in default green color of success message
        /// </summary>
        /// <param name="Text">Message</param>
        public static void UpdateStatusText(string Text)
        {
            if (sf != null)
                sf.UpdateStatusText(Text);

        }

        /// <summary>
        /// Update text with message color defined as green/yellow/red/ for success/warning/failure
        /// </summary>
        /// <param name="Text">Message</param>
        /// <param name="tom">Type of Message</param>
        public static void UpdateStatusTextWithStatus(string Text, TypeOfMessage tom)
        {

            if (sf != null)
                sf.UpdateStatusTextWithStatus(Text, tom);
        }

        public static void AdvanceProgressBar(int by = 10)
        {
            if (sf != null && sf.progressBar1.Value + by <= sf.progressBar1.MaxValue)
                sf.progressBar1.Value += by;
        }
        public static SplashScreenForm GetScreen()
        {
            return sf;
        }
    }
}
