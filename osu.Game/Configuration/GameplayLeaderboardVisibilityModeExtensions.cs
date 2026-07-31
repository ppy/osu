// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Configuration
{
    public static class GameplayLeaderboardVisibilityModeExtensions
    {
        public static bool ShouldDisplay(this GameplayLeaderboardVisibilityMode mode, bool isMultiplayer)
        {
            return mode switch
            {
                GameplayLeaderboardVisibilityMode.Never => false,
                GameplayLeaderboardVisibilityMode.Multiplayer => isMultiplayer,
                GameplayLeaderboardVisibilityMode.Always => true,
                _ => false,
            };
        }
    }
}
