// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchLeaderboardScore : LeaderboardScore
    {
        private readonly APIUserScoreAggregate score;

        public MatchLeaderboardScore(APIUserScoreAggregate score, int rank)
            : base(score.CreateScoreInfo(), rank)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RankContainer.Alpha = 0;
        }

        protected override IEnumerable<LeaderboardScoreStatistic> GetStatistics(ScoreInfo model) => new[]
        {
            new LeaderboardScoreStatistic(FontAwesome.Solid.Crosshairs, "准确率", string.Format(model.Accuracy % 1 == 0 ? @"{0:0%}" : @"{0:0.00%}", model.Accuracy)),
            new LeaderboardScoreStatistic(FontAwesome.Solid.Sync, "尝试次数", score.TotalAttempts.ToString()),
            new LeaderboardScoreStatistic(FontAwesome.Solid.Check, "通过次数", score.CompletedBeatmaps.ToString()),
        };
    }
}
