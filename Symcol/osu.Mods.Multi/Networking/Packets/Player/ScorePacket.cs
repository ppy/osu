using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Player
{
    [Serializable]
    public class ScorePacket : Packet
    {
        public long UserID;

        public int Score;
    }
}
