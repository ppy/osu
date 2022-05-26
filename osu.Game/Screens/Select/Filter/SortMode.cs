// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Select.Filter
{
    public enum SortMode
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingArtist))]
        Artist,

        [Description("Author")]
        Author,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatsBpm))]
        BPM,

        [Description("Date Added")]
        DateAdded,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingDifficulty))]
        Difficulty,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.ArtistTracksLength))]
        Length,

        // todo: pending support (https://github.com/ppy/osu/issues/4917)
        // [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchFiltersRank))]
        // RankAchieved,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowInfoSource))]
        Source,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingTitle))]
        Title,
    }
}
