// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Select.Leaderboards
{
    public enum BeatmapLeaderboardScope
    {
        [Description("Local Ranking")]
        Local,

        [Description("Country Ranking")]
        Country,

        [Description("Global Ranking")]
        Global,

        [Description("Friend Ranking")]
        Friend,
    }
}
