// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Screens.Select.Filter
{
    public enum GroupMode
    {
        [Description("All")]
        All,
        [Description("Artist")]
        Artist,
        [Description("Author")]
        Author,
        [Description("BPM")]
        BPM,
        [Description("Collections")]
        Collections,
        [Description("Date Added")]
        DateAdded,
        [Description("Difficulty")]
        Difficulty,
        [Description("Favorites")]
        Favorites,
        [Description("Length")]
        Length,
        [Description("My Maps")]
        MyMaps,
        [Description("No Grouping")]
        NoGrouping,
        [Description("Rank Achieved")]
        RankAchieved,
        [Description("Ranked Status")]
        RankedStatus,
        [Description("Recently Played")]
        RecentlyPlayed,
        [Description("Title")]
        Title
    }
}
