// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Select.Filter
{
    public enum GroupMode
    {
        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.None))]
        None,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Artist))]
        Artist,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Author))]
        Author,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.BPM))]
        BPM,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Collections))]
        Collections,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.DateAdded))]
        DateAdded,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.DateRanked))]
        DateRanked,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Difficulty))]
        Difficulty,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Favourites))]
        Favourites,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.LastPlayed))]
        LastPlayed,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Length))]
        Length,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.MyMaps))]
        MyMaps,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.RankAchieved))]
        RankAchieved,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.RankedStatus))]
        RankedStatus,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Source))]
        Source,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Title))]
        Title,
    }
}
