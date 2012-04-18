﻿//    File:        Packets.cs
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

namespace ModUpdater.Net
{

    public enum PacketId : byte
    {
        /** Server <--> Client **/
        Handshake, // <-->
        EncryptionStatus, // -->
        RequestMod, // <--
        FilePart, // <--> Notes: only sent to the server from an admin client.
        ModInfo, // -->
        ModList, // -->
        AllDone, // -->
        NextDownload, // -->
        Log, // <--
        Metadata, // <-->
        Image, // -->
        ServerList, // MasterServer --> Client
        AdminFileInfo, // --> Admin Client
        Disconnect = 255 // <-- Notes: Disconnect Packet
    }
}
