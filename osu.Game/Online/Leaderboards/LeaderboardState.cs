// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Leaderboards
{
    public enum LeaderboardState
    {
        Success,
        Retrieving,
        NoScores,

        NetworkFailure = LeaderboardFailState.NetworkFailure,
        BeatmapUnavailable = LeaderboardFailState.BeatmapUnavailable,
        RulesetUnavailable = LeaderboardFailState.RulesetUnavailable,
        NoneSelected = LeaderboardFailState.NoneSelected,
        NotLoggedIn = LeaderboardFailState.NotLoggedIn,
        NotSupporter = LeaderboardFailState.NotSupporter,
        NoTeam = LeaderboardFailState.NoTeam,
    }
}
