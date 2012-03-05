using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater
{

    public enum PacketId : byte
    {
        /** Server <--> Client **/
        Handshake, // <-->
        EncryptionStatus, // -->
        Ping, // -->
        RequestMod, // <--
        FilePart, // -->
        ModInfo, // -->
        ModList, // -->
        AllDone, // -->
        NextDownload, // -->
        Admin, // <--
        AdminUpload, // <--
        AdminInfo, // <--
        Log, // <--
        Metadata, // <-->
        Image, // -->
        BeginDownload, // <--
        Connect, // -->
        Disconnect = 255 // <-- Notes: Disconnect Packet
    }
}
