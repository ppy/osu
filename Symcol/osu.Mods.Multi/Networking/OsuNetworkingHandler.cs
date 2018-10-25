using osu.Core;
using osu.Core.Config;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Mods.Multi.Networking.Packets.Lobby;
using osu.Mods.Multi.Networking.Packets.Match;
using Symcol.Networking.NetworkingHandlers;
using Symcol.Networking.NetworkingHandlers.Peer;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking
{
    public class OsuNetworkingHandler : PeerNetworkingHandler, IOnlineComponent
    {
        protected override string Gamekey => "osu";

        public OsuUserInfo OsuUserInfo = new OsuUserInfo();

        private APIState apiState = APIState.Offline;

        private bool connectReady;
        private double forceTime = double.MinValue;

        public override void Connect()
        {
            if (apiState != APIState.Online)
            {
                connectReady = true;
                forceTime = Time.Current + 1000;
            }
            else
                connect();
        }

        private void connect()
        {
            Logger.Log($"Attempting to connect to {NetworkingClient.Address}", LoggingTarget.Network);
            SendToServer(new OsuConnectPacket());
            ConnectionStatues = ConnectionStatues.Connecting;
        }

        protected override void Update()
        {
            base.Update();

            if (forceTime <= Time.Current && ConnectionStatues <= ConnectionStatues.Disconnected)
            {
                connect();
                Logger.Log($"Joining server without logging in as ({OsuUserInfo.Username} + {OsuUserInfo.ID})", LoggingTarget.Network, LogLevel.Error);
            }
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            api.Register(this);
        }

        protected override Packet SignPacket(Packet packet)
        {
            switch (packet)
            {
                case OsuConnectPacket connectPacket:
                    connectPacket.User = OsuUserInfo;
                    break;
                case MatchPacket matchPacket:
                    matchPacket.User = OsuUserInfo;
                    break;
            }
            return base.SignPacket(packet);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            apiState = state;
            OsuUserInfo.Colour = SymcolOsuModSet.SymcolConfigManager.GetBindable<string>(SymcolSetting.PlayerColor);

            switch (state)
            {
                default:
                    OsuUserInfo.Username = SymcolOsuModSet.SymcolConfigManager.Get<string>(SymcolSetting.SavedName);
                    OsuUserInfo.ID = SymcolOsuModSet.SymcolConfigManager.Get<int>(SymcolSetting.SavedUserID);
                    break;
                case APIState.Online:
                    SymcolOsuModSet.SymcolConfigManager.Set(SymcolSetting.SavedName, api.LocalUser.Value.Username);
                    SymcolOsuModSet.SymcolConfigManager.Set(SymcolSetting.SavedUserID, api.LocalUser.Value.Id);

                    OsuUserInfo.Username = api.LocalUser.Value.Username;
                    OsuUserInfo.ID = api.LocalUser.Value.Id;
                    OsuUserInfo.Country = api.LocalUser.Value.Country.FullName;
                    OsuUserInfo.CountryFlagName = api.LocalUser.Value.Country.FlagName;
                    OsuUserInfo.Pic = api.LocalUser.Value.AvatarUrl;
                    OsuUserInfo.Background = api.LocalUser.Value.CoverUrl;

                    if (connectReady)
                        connect();
                    break;
            }
        }
    }
}
