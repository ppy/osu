// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Direction = FillDirection.Vertical;

            Spacing = new Vector2(2.5f);

            LayoutDuration = 250;
            LayoutEasing = Easing.OutQuint;
        }

        /// <summary>
        /// Adds a player to the leaderboard.
        /// </summary>
        /// <param name="currentScore">The bindable current score of the player.</param>
        /// <param name="user">The player.</param>
        public void AddPlayer([NotNull] BindableDouble currentScore, [NotNull] User user)
        {
            var scoreItem = addScore(currentScore.Value, user);
            currentScore.ValueChanged += s => scoreItem.TotalScore = s.NewValue;
        }

        private GameplayLeaderboardScore addScore(double totalScore, User user)
        {
            var scoreItem = new GameplayLeaderboardScore
            {
                User = user,
                TotalScore = totalScore,
                OnScoreChange = updateScores,
            };

            Add(scoreItem);
            updateScores();

            return scoreItem;
        }

        private void updateScores()
        {
            var orderedByScore = this.OrderByDescending(i => i.TotalScore).ToList();

            for (int i = 0; i < Count; i++)
            {
                SetLayoutPosition(orderedByScore[i], i);
                orderedByScore[i].ScorePosition = i + 1;
            }
        }
    }
}
