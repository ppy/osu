using osu.Mods.Multi.Networking;
using Symcol.Base.Graphics.Containers;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Screens.Pieces
{
    public class MultiplayerContainer : SymcolContainer
    {
        public readonly OsuNetworkingHandler OsuNetworkingHandler;

        public MultiplayerContainer(OsuNetworkingHandler osuNetworkingHandler)
        {
            OsuNetworkingHandler = osuNetworkingHandler;
            OsuNetworkingHandler.OnPacketReceive += OnPacketRecieve;
        }

        protected virtual void SendPacket(Packet packet) => OsuNetworkingHandler.SendToServer(packet);

        protected virtual void OnPacketRecieve(PacketInfo info)
        {

        }

        protected override void Dispose(bool isDisposing)
        {
            OsuNetworkingHandler.OnPacketReceive -= OnPacketRecieve;
            base.Dispose(isDisposing);
        }
    }
}
