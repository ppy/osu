// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class GameplayLeaderboard : FillFlowContainer<GameplayLeaderboardScore>
    {
        /// <summary>
        /// Whether to declare a new position for un-positioned players.
        /// Must be disabled for online leaderboards with top 50 scores only.
        /// </summary>
        public bool DeclareNewPosition = true;

        public GameplayLeaderboard()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(2.5f);
            LayoutDuration = 500;
            LayoutEasing = Easing.OutQuint;
        }

        /// <summary>
        /// Adds a real-time player score item whose score is updated via a <see cref="BindableDouble"/>.
        /// </summary>
        /// <param name="currentScore">The bindable current score of the player.</param>
        /// <param name="user">The player user.</param>
        /// <returns>Returns the drawable score item of that player.</returns>
        public GameplayLeaderboardScore AddRealTimePlayer(BindableDouble currentScore, User user = null)
        {
            if (currentScore == null)
                return null;

            var scoreItem = addScore(currentScore.Value, user);
            currentScore.ValueChanged += s => scoreItem.TotalScore = s.NewValue;

            return scoreItem;
        }

        /// <summary>
        /// Adds a score item based off a <see cref="ScoreInfo"/> with an initial position.
        /// </summary>
        /// <param name="score">The score info to use for this item.</param>
        /// <param name="initialPosition">The initial position of this item.</param>
        /// <returns>Returns the drawable score item of that player.</returns>
        public GameplayLeaderboardScore AddScore(ScoreInfo score, int? initialPosition = null) => score != null ? addScore(score.TotalScore, score.User, initialPosition) : null;

        private int maxPosition => this.Max(i => this.Any(item => item.InitialPosition.HasValue) ? i.InitialPosition : i.ScorePosition) ?? 0;

        private GameplayLeaderboardScore addScore(double totalScore, User user = null, int? position = null)
        {
            var scoreItem = new GameplayLeaderboardScore(position)
            {
                User = user,
                TotalScore = totalScore,
                OnScoreChange = updateScores,
            };

            Add(scoreItem);
            SetLayoutPosition(scoreItem, position ?? maxPosition + 1);

            reorderPositions();

            return scoreItem;
        }

        private void reorderPositions()
        {
            var orderedByScore = this.OrderByDescending(i => i.TotalScore).ToList();
            var orderedPositions = this.Select(i => this.Any(item => item.InitialPosition.HasValue) ? i.InitialPosition : i.ScorePosition).OrderByDescending(p => p.HasValue).ThenBy(p => p).ToList();

            for (int i = 0; i < Count; i++)
            {
                int newPosition = orderedPositions[i] ?? maxPosition + 1;

                SetLayoutPosition(orderedByScore[i], newPosition);
                orderedByScore[i].ScorePosition = DeclareNewPosition ? newPosition : orderedPositions[i];
            }
        }

        private void updateScores()
        {
            var orderedByScore = this.OrderByDescending(i => i.TotalScore).ToList();
            var orderedPositions = this.Select(i => this.Any(item => item.InitialPosition.HasValue) ? i.InitialPosition : i.ScorePosition).OrderByDescending(p => p.HasValue).ThenBy(p => p).ToList();

            for (int i = 0; i < Count; i++)
            {
                int newPosition = orderedPositions[i] ?? maxPosition + 1;

                SetLayoutPosition(orderedByScore[i], newPosition);
                orderedByScore[i].ScorePosition = DeclareNewPosition ? newPosition : orderedPositions[i];
            }
        }
    }
}
