// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Select.Filter
{
    public enum SortMode
    {
        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Artist))]
        Artist,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Author))]
        Author,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.BPM))]
        BPM,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.DateAdded))]
        DateAdded,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.DateRanked))]
        DateRanked,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.DateSubmitted))]
        DateSubmitted,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Difficulty))]
        Difficulty,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.LastPlayed))]
        LastPlayed,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Length))]
        Length,

        // // todo: pending support (https://github.com/ppy/osu/issues/4917)
        // [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.RankAchieved))]
        // RankAchieved,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Source))]
        Source,

        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.Title))]
        Title,
    }
}
