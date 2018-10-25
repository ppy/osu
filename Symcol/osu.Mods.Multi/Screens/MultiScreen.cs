using osu.Core.Screens.Evast;
using osu.Framework.Screens;
using osu.Mods.Multi.Networking;
using Symcol.Networking.Packets;

// ReSharper disable DelegateSubtraction

namespace osu.Mods.Multi.Screens
{
    public class MultiScreen : BeatmapScreen
    {
        public readonly OsuNetworkingHandler OsuNetworkingHandler;

        public MultiScreen(OsuNetworkingHandler osuNetworkingHandler)
        {
            OsuNetworkingHandler = osuNetworkingHandler;
        }

        protected virtual void SendPacket(Packet packet) => OsuNetworkingHandler.SendToServer(packet);

        protected virtual void OnPacketRecieve(PacketInfo info)
        {

        }

        protected override void OnEntering(Screen last)
        {
            Add(OsuNetworkingHandler);
            OsuNetworkingHandler.OnPacketReceive += OnPacketRecieve;
            base.OnEntering(last);
        }

        protected override void OnSuspending(Screen next)
        {
            Remove(OsuNetworkingHandler);
            OsuNetworkingHandler.OnPacketReceive -= OnPacketRecieve;
            base.OnSuspending(next);
        }

        protected override void OnResuming(Screen last)
        {
            Add(OsuNetworkingHandler);
            OsuNetworkingHandler.OnPacketReceive += OnPacketRecieve;
            base.OnResuming(last);
        }

        protected override bool OnExiting(Screen next)
        {
            Remove(OsuNetworkingHandler);
            OsuNetworkingHandler.OnPacketReceive -= OnPacketRecieve;
            return base.OnExiting(next);
        }
    }
}
