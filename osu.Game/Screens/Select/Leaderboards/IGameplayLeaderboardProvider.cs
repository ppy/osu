// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.Select.Leaderboards
{
    /// <summary>
    /// Provides a leaderboard to show during gameplay.
    /// </summary>
    public interface IGameplayLeaderboardProvider
    {
        /// <summary>
        /// List of all scores to display on the leaderboard.
        /// </summary>
        public IBindableList<IGameplayLeaderboardScore> Scores { get; }

        /// <summary>
        /// Whether this leaderboard is a partial leaderboard (e.g. contains only the top 50 of all scores),
        /// or is a full leaderboard (contains all scores that there will ever be).
        /// </summary>
        bool IsPartial { get; }
    }
}
