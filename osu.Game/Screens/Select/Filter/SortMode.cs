// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;
using WebBeatmapsStrings = osu.Game.Resources.Localisation.Web.BeatmapsStrings;
using WebSortStrings = osu.Game.Resources.Localisation.Web.SortStrings;

namespace osu.Game.Screens.Select.Filter
{
    public enum SortMode
    {
        [LocalisableDescription(typeof(WebBeatmapsStrings), nameof(WebBeatmapsStrings.ListingSearchSortingTitle))]
        Title,

        [LocalisableDescription(typeof(WebBeatmapsStrings), nameof(WebBeatmapsStrings.ListingSearchSortingArtist))]
        Artist,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Author))]
        Author,

        [LocalisableDescription(typeof(WebSortStrings), nameof(WebSortStrings.ArtistTracksBpm))]
        BPM,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.DateSubmitted))]
        DateSubmitted,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.DateRanked))]
        DateAdded,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.DateRanked))]
        DateRanked,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.LastPlayed))]
        LastPlayed,

        [LocalisableDescription(typeof(WebBeatmapsStrings), nameof(WebBeatmapsStrings.ListingSearchSortingDifficulty))]
        Difficulty,

        [LocalisableDescription(typeof(WebSortStrings), nameof(WebSortStrings.ArtistTracksLength))]
        Length,

        // todo: pending support (https://github.com/ppy/osu/issues/4917)
        // [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.RankAchieved))]
        // RankAchieved,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Source))]
        Source,
    }
}
