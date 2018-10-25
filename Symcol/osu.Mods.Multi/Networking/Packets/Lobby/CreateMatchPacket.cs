using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Lobby
{
    [Serializable]
    public class CreateMatchPacket : Packet
    {
        public override uint PacketSize => 2048;

        public MatchListPacket.MatchInfo MatchInfo;
    }
}
