// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Select.Filter
{
    public enum SortMode
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingArtist))]
        Artist,

        [Description("谱师")]
        Author,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.ArtistTracksBpm))]
        BPM,

        [Description("提交日期")]
        DateSubmitted,

        [Description("添加日期")]
        DateAdded,

        [Description("上架日期")]
        DateRanked,

        [Description("上次游玩")]
        LastPlayed,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingDifficulty))]
        Difficulty,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.ArtistTracksLength))]
        Length,

        // todo: pending support (https://github.com/ppy/osu/issues/4917)
        // [Description("Rank Achieved")]
        // RankAchieved,

        [Description("来源")]
        Source,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingTitle))]
        Title,
    }
}
