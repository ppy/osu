// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Play.HUD
{
    public class SoloGameplayLeaderboard : GameplayLeaderboard
    {
        private readonly IUser trackingUser;

        public readonly IBindableList<ScoreInfo> Scores = new BindableList<ScoreInfo>();

        // hold references to ensure bindables are updated.
        private readonly List<Bindable<long>> scoreBindables = new List<Bindable<long>>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        public SoloGameplayLeaderboard(IUser trackingUser)
        {
            this.trackingUser = trackingUser;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scores.BindCollectionChanged((_, _) => Scheduler.AddOnce(showScores), true);
        }

        private void showScores()
        {
            Clear();
            scoreBindables.Clear();

            if (!Scores.Any())
                return;

            ILeaderboardScore local = Add(trackingUser, true);

            local.TotalScore.BindTarget = scoreProcessor.TotalScore;
            local.Accuracy.BindTarget = scoreProcessor.Accuracy;
            local.Combo.BindTarget = scoreProcessor.Combo;

            // Local score should always show lower than any existing scores in cases of ties.
            local.DisplayOrder.Value = long.MaxValue;

            foreach (var s in Scores)
            {
                var score = Add(s.User, false);

                var bindableTotal = scoreManager.GetBindableTotalScore(s);

                // Direct binding not possible due to differing types (see https://github.com/ppy/osu/issues/20298).
                bindableTotal.BindValueChanged(total => score.TotalScore.Value = total.NewValue, true);
                scoreBindables.Add(bindableTotal);

                score.Accuracy.Value = s.Accuracy;
                score.Combo.Value = s.MaxCombo;
                score.DisplayOrder.Value = s.OnlineID > 0 ? s.OnlineID : s.Date.ToUnixTimeSeconds();
            }
        }
    }
}
