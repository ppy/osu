// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Select.Leaderboards
{
    public enum BeatmapLeaderboardScope
    {
        [Description("本地排名")]
        Local,

        [Description("国内排名")]
        Country,

        [Description("全球排名")]
        Global,

        [Description("好友排名")]
        Friend,
    }
}
