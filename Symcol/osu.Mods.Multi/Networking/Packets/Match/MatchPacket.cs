using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Match
{
    [Serializable]
    public class MatchPacket : Packet
    {
        public override uint PacketSize => 1024;

        public OsuUserInfo User;
    }
}
