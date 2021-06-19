// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchGeneralEnumLocalisationMapper))]
    public enum SearchGeneral
    {
        [Description("推荐难度")]
        Recommended,

        [Description("包括转谱")]
        Converts,

        [Description("已关注的谱师")]
        Follows
    }

    public class SearchGeneralEnumLocalisationMapper : EnumLocalisationMapper<SearchGeneral>
    {
        public override LocalisableString Map(SearchGeneral value)
        {
            switch (value)
            {
                case SearchGeneral.Recommended:
                    return BeatmapsStrings.GeneralRecommended;

                case SearchGeneral.Converts:
                    return BeatmapsStrings.GeneralConverts;

                case SearchGeneral.Follows:
                    return BeatmapsStrings.GeneralFollows;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
