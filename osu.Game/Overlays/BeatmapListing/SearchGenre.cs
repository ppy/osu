// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchGenre
    {
        [Description("任意")]
        Any = 0,
        [Description("未指定")]
        Unspecified = 1,

        [Description("游戏")]
        VideoGame = 2,
        [Description("动漫")]
        Anime = 3,
        [Description("摇滚")]
        Rock = 4,
        [Description("流行")]
        Pop = 5,
        [Description("其他")]
        Other = 6,
        [Description("新奇")]
        Novelty = 7,

        [Description("嘻哈")]
        HipHop = 9,
        [Description("电子")]
        Electronic = 10,
        [Description("金属")]
        Metal = 11,
        [Description("古典")]
        Classical = 12,
        [Description("民歌")]
        Folk = 13,
        [Description("爵士")]
        Jazz = 14
    }
}
