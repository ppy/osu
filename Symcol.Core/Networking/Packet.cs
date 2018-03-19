using System;

namespace Symcol.Core.Networking
{
    [Serializable]
    public class Packet
    {
        /// <summary>
        /// Just a Signature
        /// </summary>
        public readonly ClientInfo ClientInfo;

        /// <summary>
        /// Specify starting size of packet for efficiency
        /// </summary>
        public virtual int PacketSize => 1024;

        public Packet(ClientInfo clientInfo)
        {
            ClientInfo = clientInfo;
        }
    }
}
