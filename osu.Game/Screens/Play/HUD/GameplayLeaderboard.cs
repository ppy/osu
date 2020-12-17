// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class GameplayLeaderboard : FillFlowContainer<GameplayLeaderboardScore>
    {
        public GameplayLeaderboard()
        {
            AutoSizeAxes = Axes.Both;

            Direction = FillDirection.Vertical;

            Spacing = new Vector2(2.5f);

            LayoutDuration = 250;
            LayoutEasing = Easing.OutQuint;
        }

        /// <summary>
        /// Adds a player to the leaderboard.
        /// </summary>
        /// <param name="totalScore">A bindable of the player's total score.</param>
        /// <param name="accuracy">A bindable of the player's accuracy.</param>
        /// <param name="combo">A bindable of the player's current combo.</param>
        /// <param name="user">The player.</param>
        public void AddPlayer([NotNull] IBindableNumber<double> totalScore,
                              [NotNull] IBindableNumber<double> accuracy,
                              [NotNull] IBindableNumber<int> combo,
                              [NotNull] User user)
        {
            AddPlayer(totalScore, accuracy, combo, user, false);
        }

        /// <summary>
        /// Adds a player to the leaderboard.
        /// </summary>
        /// <param name="totalScore">A bindable of the player's total score.</param>
        /// <param name="accuracy">A bindable of the player's accuracy.</param>
        /// <param name="combo">A bindable of the player's current combo.</param>
        /// <param name="user">The player.</param>
        /// <param name="localUser">Whether the provided <paramref name="user"/> is the local user.</param>
        protected void AddPlayer([NotNull] IBindableNumber<double> totalScore,
                                 [NotNull] IBindableNumber<double> accuracy,
                                 [NotNull] IBindableNumber<int> combo,
                                 [NotNull] User user, bool localUser) => Schedule(() =>
        {
            Add(new GameplayLeaderboardScore(user, localUser)
            {
                TotalScore = { BindTarget = totalScore },
                Accuracy = { BindTarget = accuracy },
                Combo = { BindTarget = combo },
            });

            totalScore.BindValueChanged(_ => updateScores(), true);
        });

        private void updateScores()
        {
            var orderedByScore = this.OrderByDescending(i => i.TotalScore.Value).ToList();

            for (int i = 0; i < Count; i++)
            {
                SetLayoutPosition(orderedByScore[i], i);
                orderedByScore[i].ScorePosition = i + 1;
            }
        }
    }
}
