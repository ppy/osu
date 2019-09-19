// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
