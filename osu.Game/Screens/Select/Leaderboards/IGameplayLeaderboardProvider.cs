// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

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
        public IBindableList<GameplayLeaderboardScore> Scores { get; }

        /// <summary>
        /// Whether the scores on this leaderboard have pre-existing initial positions.
        /// This will be <see langword="true"/> if the scores have been fetched from online beatmap leaderboards.
        /// </summary>
        /// <remarks>
        /// If this is <see langword="true"/> and a tracked score is not precisely between two subsequent scores with respect to initial position (e.g. #3 and #4),
        /// it will show an "unknown" score position.
        /// </remarks>
        bool HasInitialScorePositions { get; }
    }
}
