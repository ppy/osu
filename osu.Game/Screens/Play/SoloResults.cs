// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Types;

namespace osu.Game.Screens.Play
{
    public class SoloResults : Results
    {
        [Resolved]
        ScoreManager scores { get; set; }

        public SoloResults(ScoreInfo score)
            : base(score)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (scores.IsAvailableLocally(Score) || hasOnlineReplay)
            {
                AddInternal(new ReplayDownloadButton(Score)
                {
                    Anchor = Framework.Graphics.Anchor.BottomRight,
                    Origin = Framework.Graphics.Anchor.BottomRight,
                    Height = 80,
                    Width = 100,
                });
            }
        }

        private bool hasOnlineReplay => Score is APILegacyScoreInfo apiScore && apiScore.OnlineScoreID != null && apiScore.Replay;

        protected override IEnumerable<IResultPageInfo> CreateResultPages() => new IResultPageInfo[]
        {
            new ScoreOverviewPageInfo(Score, Beatmap.Value),
            new LocalLeaderboardPageInfo(Score, Beatmap.Value)
        };
    }
}
