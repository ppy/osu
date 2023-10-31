// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SortCriteria
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingTitle))]
        Title,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingArtist))]
        Artist,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingDifficulty))]
        Difficulty,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingUpdated))]
        Updated,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingRanked))]
        Ranked,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingRating))]
        Rating,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingPlays))]
        Plays,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingFavourites))]
        Favourites,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingRelevance))]
        Relevance,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingNominations))]
        Nominations,
    }
}
