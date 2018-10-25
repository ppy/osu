using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Lobby
{
    [Serializable]
    public class JoinMatchPacket : Packet
    {
        public override uint PacketSize => 2048;

        public OsuUserInfo User;

        public MatchListPacket.MatchInfo Match;
    }
}
