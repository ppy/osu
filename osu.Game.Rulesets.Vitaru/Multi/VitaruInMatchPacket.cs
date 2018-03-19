using Symcol.Core.Networking;
using System;

namespace osu.Game.Rulesets.Vitaru.Multi
{
    [Serializable]
    public class VitaruInMatchPacket : Packet
    {
        /// <summary>
        /// This player's information
        /// </summary>
        public VitaruPlayerInformation PlayerInformation;

        public override int PacketSize => 2048;

        public VitaruInMatchPacket(ClientInfo clientInfo) : base(clientInfo)
        {

        }
    }
}
