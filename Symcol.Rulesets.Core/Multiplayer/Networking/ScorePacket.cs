using Symcol.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symcol.Rulesets.Core.Multiplayer.Networking
{
    [Serializable]
    public class ScorePacket : Packet
    {
        public override int PacketSize => 2048;

        public int Score;

        public ScorePacket(ClientInfo clientInfo, int score) : base(clientInfo)
        {
            Score = score;
        }
    }
}
