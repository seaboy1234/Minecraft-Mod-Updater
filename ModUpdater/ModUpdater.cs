﻿//    File:        ModUpdater.cs
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
using ModUpdater.Utility;

namespace ModUpdater
{
    /// <summary>
    /// Class contains basic information about the app
    /// </summary>
    public static class MinecraftModUpdater
    {
        /// <summary>
        /// The shortname.
        /// </summary>
        public const string ShortAppName = "MCModUpdater";
        /// <summary>
        /// The name.
        /// </summary>
        public const string LongAppName = "Minecraft Mod Updater";
        /// <summary>
        /// The Mod Updater Version.
        /// </summary>
        public const string Version = "1.2.1_dev";
        /// <summary>
        /// The branch of the Git repo.
        /// </summary>
        public const string Branch = "develop";
        /// <summary>
        /// The Logger.
        /// </summary>
        public static readonly Logger Logger = new Logger();
    }
}
