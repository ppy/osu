// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public class MatchLeaderboardScore : LeaderboardScore
    {
        private readonly APIUserScoreAggregate score;

        public MatchLeaderboardScore(APIUserScoreAggregate score, int? rank, bool allowHighlight = true)
            : base(score.CreateScoreInfo(), rank, allowHighlight)
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
            new LeaderboardScoreStatistic(FontAwesome.Solid.Crosshairs, "Accuracy", model.DisplayAccuracy),
            new LeaderboardScoreStatistic(FontAwesome.Solid.Sync, "Total Attempts", score.TotalAttempts.ToString()),
            new LeaderboardScoreStatistic(FontAwesome.Solid.Check, "Completed Beatmaps", score.CompletedBeatmaps.ToString()),
        };
    }
}
