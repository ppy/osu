// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
{
    public partial class BeatmapLeaderboard : Leaderboard<BeatmapLeaderboardScope, ScoreInfo>
    {

        public Action<ScoreInfo>? ScoreSelected;

        public BeatmapLeaderboard(BeatmapLeaderboardScoresProvider leaderboardScoresProvider) : base(leaderboardScoresProvider)
        {
        }

        protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index) => new LeaderboardScore(model, index, LeaderboardScoresProvider.IsOnlineScope)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };

        protected override LeaderboardScore CreateDrawableTopScore(ScoreInfo model) => new LeaderboardScore(model, model.Position, false)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };
    }
}
