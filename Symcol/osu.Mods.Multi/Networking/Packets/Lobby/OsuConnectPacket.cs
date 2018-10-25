using System;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Lobby
{
    [Serializable]
    public class OsuConnectPacket : ConnectPacket
    {
        public override uint PacketSize => 1024;

        public OsuUserInfo User;
    }
}
