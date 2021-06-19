// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchCategoryEnumLocalisationMapper))]
    public enum SearchCategory
    {
        [Description("所有谱面")]
        Any,

        [Description("拥有排行榜的谱面")]
        Leaderboard,
        [Description("计入排名的谱面")]
        Ranked,
        [Description("质量合格的谱面")]
        Qualified,
        [Description("Loved谱面")]
        Loved,
        [Description("喜欢的谱面")]
        Favourites,

        [Description("审核中、制作中的谱面")]
        Pending,
        [Description("坟图")]
        Graveyard,

        [Description("我制作的谱面")]
        Mine,
    }

    public class SearchCategoryEnumLocalisationMapper : EnumLocalisationMapper<SearchCategory>
    {
        public override LocalisableString Map(SearchCategory value)
        {
            switch (value)
            {
                case SearchCategory.Any:
                    return BeatmapsStrings.StatusAny;

                case SearchCategory.Leaderboard:
                    return BeatmapsStrings.StatusLeaderboard;

                case SearchCategory.Ranked:
                    return BeatmapsStrings.StatusRanked;

                case SearchCategory.Qualified:
                    return BeatmapsStrings.StatusQualified;

                case SearchCategory.Loved:
                    return BeatmapsStrings.StatusLoved;

                case SearchCategory.Favourites:
                    return BeatmapsStrings.StatusFavourites;

                case SearchCategory.Pending:
                    return BeatmapsStrings.StatusPending;

                case SearchCategory.Graveyard:
                    return BeatmapsStrings.StatusGraveyard;

                case SearchCategory.Mine:
                    return BeatmapsStrings.StatusMine;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
