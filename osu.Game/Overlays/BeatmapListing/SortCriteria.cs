// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SortCriteriaLocalisationMapper))]
    public enum SortCriteria
    {
        [Description("标题")]
        Title,
        [Description("艺术家")]
        Artist,
        [Description("难度较高的")]
        Difficulty,
        [Description("计入排名的")]
        Ranked,
        [Description("评分较高的")]
        Rating,
        [Description("游玩较多的")]
        Plays,
        [Description("我喜欢的")]
        Favourites,
        [Description("相关性")]
        Relevance,
    }

    public class SortCriteriaLocalisationMapper : EnumLocalisationMapper<SortCriteria>
    {
        public override LocalisableString Map(SortCriteria value)
        {
            switch (value)
            {
                case SortCriteria.Title:
                    return BeatmapsStrings.ListingSearchSortingTitle;

                case SortCriteria.Artist:
                    return BeatmapsStrings.ListingSearchSortingArtist;

                case SortCriteria.Difficulty:
                    return BeatmapsStrings.ListingSearchSortingDifficulty;

                case SortCriteria.Ranked:
                    return BeatmapsStrings.ListingSearchSortingRanked;

                case SortCriteria.Rating:
                    return BeatmapsStrings.ListingSearchSortingRating;

                case SortCriteria.Plays:
                    return BeatmapsStrings.ListingSearchSortingPlays;

                case SortCriteria.Favourites:
                    return BeatmapsStrings.ListingSearchSortingFavourites;

                case SortCriteria.Relevance:
                    return BeatmapsStrings.ListingSearchSortingRelevance;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
