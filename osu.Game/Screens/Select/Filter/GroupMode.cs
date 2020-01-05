// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Select.Filter
{
    public enum GroupMode
    {
        [Description("所有")]
        All,

        [Description("艺术家")]
        Artist,

        [Description("作图者")]
        Author,

        [Description("BPM")]
        BPM,

        [Description("收藏夹")]
        Collections,

        [Description("添加日期")]
        DateAdded,

        [Description("难度")]
        Difficulty,

        [Description("我喜欢的")]
        Favourites,

        [Description("长度")]
        Length,

        [Description("我的谱面")]
        MyMaps,

        [Description("不分组")]
        NoGrouping,

        [Description("达成的排名")]
        RankAchieved,

        [Description("Rank状态")]
        RankedStatus,

        [Description("最近游玩")]
        RecentlyPlayed,

        [Description("标题")]
        Title
    }
}
