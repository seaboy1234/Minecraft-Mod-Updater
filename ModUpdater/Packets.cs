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
        ClientUpdate, // -->
        Kick, // -->
        Image, // -->  Notes: To be sent on second connection.
        Connect, // --> Notes: Used to tell the client to make the second connection.
        GoodBye, // <--
        Disconnect = 255 // <-- Notes: Disconnect Packet
    }
}
