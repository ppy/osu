// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics.User;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class UserStatisticsPanel : StatisticsPanel
    {
        private readonly ScoreInfo achievedScore;

        public UserStatisticsPanel(ScoreInfo achievedScore)
        {
            this.achievedScore = achievedScore;
        }

        protected override ICollection<StatisticItem> CreateStatisticItems(ScoreInfo newScore, IBeatmap playableBeatmap)
        {
            var items = base.CreateStatisticItems(newScore, playableBeatmap);

            if (newScore.UserID > 1
                && newScore.UserID == achievedScore.UserID
                && newScore.OnlineID > 0
                && newScore.OnlineID == achievedScore.OnlineID)
            {
                items = items.Append(new StatisticItem("Overall Ranking", () => new OverallRanking(newScore)
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                })).ToArray();
            }

            return items;
        }
    }
}
