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

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.ArtistTracksBpm))]
        BPM,

        [Description("Date Submitted")]
        DateSubmitted,

        [Description("Date Added")]
        DateAdded,

        [Description("Date Ranked")]
        DateRanked,

        [Description("Last Played")]
        LastPlayed,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingDifficulty))]
        Difficulty,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.ArtistTracksLength))]
        Length,

        // todo: pending support (https://github.com/ppy/osu/issues/4917)
        // [Description("Rank Achieved")]
        // RankAchieved,

        [Description("Source")]
        Source,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingTitle))]
        Title,
    }
}
