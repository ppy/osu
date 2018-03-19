using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using System.Collections.Generic;
using osu.Framework.Graphics;
using OpenTK;
using Symcol.Core.Networking;
using Symcol.Rulesets.Core.Multiplayer.Networking;

namespace Symcol.Rulesets.Core.Multiplayer.Pieces
{
    public class MatchPlayerList : Container
    {
        private readonly RulesetNetworkingClientHandler rulesetNetworkingClientHandler;

        public readonly List<MatchPlayer> MatchPlayers = new List<MatchPlayer>();

        public readonly FillFlowContainer MatchPlayersContianer;

        public MatchPlayerList(RulesetNetworkingClientHandler rulesetNetworkingClientHandler)
        {
            this.rulesetNetworkingClientHandler = rulesetNetworkingClientHandler;

            Masking = true;
            CornerRadius = 16;
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            RelativeSizeAxes = Axes.Both;
            Width = 0.49f;
            Height = 0.45f;
            Position = new Vector2(10);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.8f)
                },
                MatchPlayersContianer = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.98f,
                    Height = 0.96f
                }
            };

            rulesetNetworkingClientHandler.OnReceivePlayerList += (players) =>
            {
                restart:
                foreach (MatchPlayer matchPlayer in MatchPlayers)
                    foreach (ClientInfo clientInfo in players)
                        if (clientInfo is RulesetClientInfo rulesetClientInfo)
                            if (rulesetClientInfo.IP + rulesetClientInfo.Port != matchPlayer.ClientInfo.IP + matchPlayer.ClientInfo.Port)
                            {
                                Add(rulesetClientInfo);
                                players.Remove(clientInfo);
                                goto restart;
                            }
            };
            rulesetNetworkingClientHandler.RequestPlayerList();

            rulesetNetworkingClientHandler.OnClientJoin += (clientInfo) =>
            {
                foreach (MatchPlayer matchPlayer in MatchPlayers)
                    if (clientInfo is RulesetClientInfo rulesetClientInfo)
                        if (rulesetClientInfo.IP + rulesetClientInfo.Port != matchPlayer.ClientInfo.IP + matchPlayer.ClientInfo.Port)
                        {
                            Add(rulesetClientInfo);
                            break;
                        }
            };

            rulesetNetworkingClientHandler.OnClientDisconnect += (clientInfo) =>
            {
                foreach (MatchPlayer matchPlayer in MatchPlayers)
                    if (clientInfo is RulesetClientInfo rulesetClientInfo)
                        if (rulesetClientInfo.IP + rulesetClientInfo.Port == matchPlayer.ClientInfo.IP + matchPlayer.ClientInfo.Port)
                        {
                            Remove(matchPlayer);
                            break;
                        }
            };
        }

        public void Add(RulesetClientInfo clientInfo)
        {
            MatchPlayer matchPlayer = new MatchPlayer(clientInfo);

            Add(matchPlayer);
        }

        public void Add(MatchPlayer matchPlayer)
        {
            MatchPlayers.Add(matchPlayer);
            MatchPlayersContianer.Add(matchPlayer);
            matchPlayer.FadeInFromZero(200);
        }

        public void Remove(MatchPlayer matchPlayer)
        {
            MatchPlayers.Remove(matchPlayer);
            matchPlayer.FadeOutFromOne(200)
                .Expire();
        }
    }
}
