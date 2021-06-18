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
        Any,

        [Description("Has Leaderboard")]
        Leaderboard,
        Ranked,
        Qualified,
        Loved,
        Favourites,

        [Description("Pending & WIP")]
        Pending,
        Graveyard,

        [Description("My Maps")]
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
