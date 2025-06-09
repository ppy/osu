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
        IBindableList<GameplayLeaderboardScore> Scores { get; }
    }

    public class EmptyGameplayLeaderboardProvider : IGameplayLeaderboardProvider
    {
        public IBindableList<GameplayLeaderboardScore> Scores { get; } = new BindableList<GameplayLeaderboardScore>();
    }
}
