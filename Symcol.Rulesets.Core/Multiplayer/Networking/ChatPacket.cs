using Symcol.Core.Networking;
using System;

namespace Symcol.Rulesets.Core.Multiplayer.Networking
{
    [Serializable]
    public class ChatPacket : Packet
    {
        public override int PacketSize => 4096;

        public string Author;

        public string AuthorColor;

        public string Message;

        public ChatPacket(ClientInfo clientInfo) : base(clientInfo)
        {

        }
    }
}
