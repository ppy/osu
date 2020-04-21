// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SortCriteria
    {
        [Description("标题")]
        Title,
        [Description("艺术家")]
        Artist,
        [Description("难度较高的")]
        Difficulty,
        [Description("计入排名的")]
        Ranked,
        [Description("评分较高的")]
        Rating,
        [Description("游玩较多的")]
        Plays,
        [Description("我喜欢的")]
        Favourites,
        [Description("相关性")]
        Relevance,
    }
}