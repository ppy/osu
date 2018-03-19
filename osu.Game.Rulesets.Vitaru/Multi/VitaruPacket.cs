using Symcol.Core.Networking;
using System;

namespace osu.Game.Rulesets.Vitaru.Multi
{
    [Serializable]
    public class VitaruPacket : Packet
    {
        public new readonly VitaruClientInfo ClientInfo;

        public override int PacketSize => 8192;

        /// <summary>
        /// Changing Character?
        /// </summary>
        public bool ChangeCharacter;

        public VitaruPacket(VitaruClientInfo vitaruClientInfo) : base(vitaruClientInfo)
        {
            ClientInfo = vitaruClientInfo;
        }
    }
}
