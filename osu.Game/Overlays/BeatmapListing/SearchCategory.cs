// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchCategory
    {
        [Description("所有谱面")]
        Any,

        [Description("拥有排行榜的谱面")]
        Leaderboard,
        [Description("计入排名的谱面")]
        Ranked,
        [Description("质量合格的谱面")]
        Qualified,
        [Description("Loved谱面")]
        Loved,
        [Description("喜欢的谱面")]
        Favourites,

        [Description("审核中、制作中的谱面")]
        Pending,
        [Description("坟图")]
        Graveyard,

        [Description("我制作的谱面")]
        Mine,
    }
}