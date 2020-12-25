// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class GameplayLeaderboard : FillFlowContainer<GameplayLeaderboardScore>
    {
        private readonly Cached sorting = new Cached();

        public GameplayLeaderboard()
        {
            Width = GameplayLeaderboardScore.EXTENDED_WIDTH + GameplayLeaderboardScore.SHEAR_WIDTH;

            Direction = FillDirection.Vertical;

            Spacing = new Vector2(2.5f);

            LayoutDuration = 250;
            LayoutEasing = Easing.OutQuint;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(sort, 1000, true);
        }

        /// <summary>
        /// Adds a player to the leaderboard.
        /// </summary>
        /// <param name="user">The player.</param>
        /// <param name="isTracked">
        /// Whether the player should be tracked on the leaderboard.
        /// Set to <c>true</c> for the local player or a player whose replay is currently being played.
        /// </param>
        public ILeaderboardScore AddPlayer(User user, bool isTracked)
        {
            var drawable = new GameplayLeaderboardScore(user, isTracked)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            base.Add(drawable);
            drawable.TotalScore.BindValueChanged(_ => sorting.Invalidate(), true);

            Height = Count * (GameplayLeaderboardScore.PANEL_HEIGHT + Spacing.Y);

            return drawable;
        }

        public sealed override void Add(GameplayLeaderboardScore drawable)
        {
            throw new NotSupportedException($"Use {nameof(AddPlayer)} instead.");
        }

        private void sort()
        {
            if (sorting.IsValid)
                return;

            var orderedByScore = this.OrderByDescending(i => i.TotalScore.Value).ToList();

            for (int i = 0; i < Count; i++)
            {
                SetLayoutPosition(orderedByScore[i], i);
                orderedByScore[i].ScorePosition = i + 1;
            }

            sorting.Validate();
        }
    }
}
