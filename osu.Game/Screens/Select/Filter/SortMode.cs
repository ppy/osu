// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Screens.Select.Filter
{
    public enum SortMode
    {
        [Description("Artist")]
        Artist,
        [Description("Author")]
        Author,
        [Description("BPM")]
        BPM,
        [Description("Date Added")]
        DateAdded,
        [Description("Difficulty")]
        Difficulty,
        [Description("Length")]
        Length,
        [Description("Rank Achieved")]
        RankAchieved,
        [Description("Title")]
        Title
    }
}
