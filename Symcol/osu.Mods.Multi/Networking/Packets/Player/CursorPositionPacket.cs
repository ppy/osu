using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Player
{
    [Serializable]
    public class CursorPositionPacket : Packet
    {
        public long ID;
        public float X;
        public float Y;
    }
}

