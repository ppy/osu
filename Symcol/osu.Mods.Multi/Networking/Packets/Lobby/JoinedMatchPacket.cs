using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Lobby
{
    [Serializable]
    public class JoinedMatchPacket : Packet
    {
        public override uint PacketSize => Convert.ToUInt32(MatchInfo.Users.Count > 0 ? MatchInfo.Users.Count * 1024 + 1024 : 2048);

        public MatchListPacket.MatchInfo MatchInfo;
    }
}
