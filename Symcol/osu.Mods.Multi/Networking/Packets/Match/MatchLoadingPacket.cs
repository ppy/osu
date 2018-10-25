using System;
using System.Collections.Generic;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Match
{
    [Serializable]
    public class MatchLoadingPacket : Packet
    {
        public override uint PacketSize => Convert.ToUInt32(Users.Count > 0 ? Users.Count * 1024 : 1024);

        public List<OsuUserInfo> Users = new List<OsuUserInfo>();
    }
}
