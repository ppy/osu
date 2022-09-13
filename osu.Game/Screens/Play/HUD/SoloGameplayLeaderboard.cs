// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Play.HUD
{
    public class SoloGameplayLeaderboard : GameplayLeaderboard
    {
        private readonly IUser trackingUser;

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public SoloGameplayLeaderboard(IUser trackingUser)
        {
            this.trackingUser = trackingUser;
        }

        public void ShowScores(IEnumerable<IScoreInfo> scores)
        {
            Clear();

            if (!scores.Any())
                return;

            ILeaderboardScore local = Add(trackingUser, true);

            local.TotalScore.BindTarget = scoreProcessor.TotalScore;
            local.Accuracy.BindTarget = scoreProcessor.Accuracy;
            local.Combo.BindTarget = scoreProcessor.Combo;

            foreach (var s in scores)
            {
                var score = Add(s.User, false);

                score.TotalScore.Value = s.TotalScore;
                score.Accuracy.Value = s.Accuracy;
                score.Combo.Value = s.MaxCombo;
            }
        }
    }
}
