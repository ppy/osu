using OpenTK;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.Scoring;
using Symcol.Core.Networking;
using Symcol.Rulesets.Core.Multiplayer.Networking;
using System.Collections.Generic;

namespace Symcol.Rulesets.Core.Multiplayer.Pieces
{
    public class MultiplayerScoreboard : Container
    {
        public readonly Container<MultiplayerScoreboardItem> ScoreboardItems;

        private readonly RulesetNetworkingClientHandler rulesetNetworkingClientHandler;

        private readonly ScoreProcessor scoreProcessor;

        private double updateScoreTime = 0;

        public MultiplayerScoreboard(RulesetNetworkingClientHandler rulesetNetworkingClientHandler, List<ClientInfo> playerList, ScoreProcessor scoreProcessor)
        {
            this.rulesetNetworkingClientHandler = rulesetNetworkingClientHandler;
            this.scoreProcessor = scoreProcessor;

            AlwaysPresent = true;
            AutoSizeAxes = Axes.Y;
            Width = 120;

            Position = new Vector2(0, -200);
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.TopLeft;

            Child = ScoreboardItems = new Container<MultiplayerScoreboardItem>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            int i = 1;
            foreach (ClientInfo clientInfo in playerList)
            {
                if (clientInfo is RulesetClientInfo rulesetClientInfo)
                {
                    ScoreboardItems.Add(new MultiplayerScoreboardItem(rulesetClientInfo, i) { });
                    i++;
                }
            }

            rulesetNetworkingClientHandler.OnPacketReceive += (Packet packet) =>
            {
                if (packet is ScorePacket scorePacket)
                {
                    rulesetNetworkingClientHandler.ShareWithOtherPeers(scorePacket);
                    foreach (MultiplayerScoreboardItem item in ScoreboardItems)
                        if (scorePacket.ClientInfo == item.ClientInfo)
                            item.Score = scorePacket.Score;
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= updateScoreTime)
            {
                updateScoreTime = Time.Current + 500;
                foreach (MultiplayerScoreboardItem item in ScoreboardItems)
                    if (rulesetNetworkingClientHandler.ClientInfo == item.ClientInfo)
                        item.Score = (int)scoreProcessor.TotalScore.Value;

                rulesetNetworkingClientHandler.SendToHost(new ScorePacket(rulesetNetworkingClientHandler.ClientInfo, (int)scoreProcessor.TotalScore.Value));
                rulesetNetworkingClientHandler.SendToInGameClients(new ScorePacket(rulesetNetworkingClientHandler.ClientInfo, (int)scoreProcessor.TotalScore.Value));
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Tab)
            {
                if (Alpha > 0)
                    this.FadeOut(100);
                else
                    this.FadeIn(100);
            }

            return base.OnKeyDown(state, args);
        }
    }
}
