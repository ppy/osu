using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Lobby
{
    [Serializable]
    public class GetMatchListPacket : Packet
    {
        public override uint PacketSize => 256;
    }
}
