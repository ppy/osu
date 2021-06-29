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
        [Description("Recommended difficulty")]
        Recommended,

        [Description("Include converted beatmaps")]
        Converts,

        [Description("Subscribed mappers")]
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
