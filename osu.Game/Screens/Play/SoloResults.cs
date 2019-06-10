// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Types;

namespace osu.Game.Screens.Play
{
    public class SoloResults : Results
    {
        private ScoreManager scores;

        public SoloResults(ScoreInfo score)
            : base(score)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, ScoreManager scores)
        {
            this.scores = scores;

            if (scores.Query(s => s.ID == Score.ID) != null || hasOnlineReplay)
            {
                AddInternal(new TwoLayerButton
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    BackgroundColour = colours.Yellow,
                    Icon = FontAwesome.Solid.PlayCircle,
                    Text = @"Replay",
                    HoverColour = colours.YellowDark,
                    Action = onReplay,
                });
            }
        }

        private bool hasOnlineReplay => Score is APILegacyScoreInfo apiScore && apiScore.OnlineScoreID != null && apiScore.Replay;

        private void onReplay()
        {
            if (Score is APILegacyScoreInfo score)
            {
                this.Push(new ReplayPlayerLoader(score, createReplayPlayer));
                return;
            }

            this.Push(new PlayerLoader(() => createReplayPlayer(scores.GetScore(Score))));
        }

        private ReplayPlayer createReplayPlayer(Score score) => new ReplayPlayer(score, true, false);

        protected override IEnumerable<IResultPageInfo> CreateResultPages() => new IResultPageInfo[]
        {
            new ScoreOverviewPageInfo(Score, Beatmap.Value),
            new LocalLeaderboardPageInfo(Score, Beatmap.Value)
        };
    }
}
