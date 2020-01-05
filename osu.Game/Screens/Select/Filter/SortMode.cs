// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Select.Filter
{
    public enum SortMode
    {
        [Description("艺术家")]
        Artist,

        [Description("作图者")]
        Author,

        [Description("BPM")]
        BPM,

        [Description("添加日期")]
        DateAdded,

        [Description("难度")]
        Difficulty,

        [Description("长度")]
        Length,

        [Description("达成的排名")]
        RankAchieved,

        [Description("标题")]
        Title
    }
}
