using osu.Framework.Allocation;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using Symcol.Core.Networking;
using System;

namespace Symcol.Rulesets.Core.Multiplayer.Networking
{
    //TODO: This NEEDS its own clock to avoid fuckery later on with DoubleTime and HalfTime
    public class RulesetNetworkingClientHandler : NetworkingClientHandler, IOnlineComponent
    {
        public RulesetClientInfo RulesetClientInfo;

        public Action<WorkingBeatmap> OnMapChange;

        private OsuGame osu;

        public RulesetNetworkingClientHandler(ClientType type, string ip, int port = 25570, string thisLocalIp = "0.0.0.0") : base(type, ip, port, thisLocalIp)
        {
            if (RulesetClientInfo == null)
            {
                RulesetClientInfo = new RulesetClientInfo
                {
                    Port = port
                };

                ClientInfo = RulesetClientInfo;
            }
        }

        /// <summary>
        /// Send Map to Peers
        /// </summary>
        /// <param name="map"></param>
        public void SetMap(WorkingBeatmap map)
        {
            RulesetPacket packet;
            try
            {
                packet = new RulesetPacket(RulesetClientInfo)
                {
                    OnlineBeatmapSetID = (int)map.BeatmapSetInfo.OnlineBeatmapSetID,
                    OnlineBeatmapID = (int)map.BeatmapInfo.OnlineBeatmapID
                };
                SendToInMatchClients(packet);
                OnMapChange?.Invoke(osu.Beatmap.Value);
            }
            catch
            {
                packet = new RulesetPacket(RulesetClientInfo);
                SendToInMatchClients(packet);
                return;
            }
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, OsuGame osu)
        {
            api.Register(this);
            this.osu = osu;
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    RulesetClientInfo.Username = "";
                    RulesetClientInfo.UserID = -1;
                    break;
                case APIState.Online:
                    RulesetClientInfo.Username = api.LocalUser.Value.Username;
                    RulesetClientInfo.UserID = (int)api.LocalUser.Value.Id;
                    RulesetClientInfo.UserCountry = api.LocalUser.Value.Country.FullName;
                    RulesetClientInfo.CountryFlagName = api.LocalUser.Value.Country.FlagName;
                    RulesetClientInfo.UserPic = api.LocalUser.Value.AvatarUrl;
                    RulesetClientInfo.UserBackground = api.LocalUser.Value.CoverUrl;
                    break;
            }
        }
    }
}
