// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchExplicitEnumLocalisationMapper))]
    public enum SearchExplicit
    {
        Hide,
        Show
    }

    public class SearchExplicitEnumLocalisationMapper : EnumLocalisationMapper<SearchExplicit>
    {
        public override LocalisableString Map(SearchExplicit value)
        {
            switch (value)
            {
                case SearchExplicit.Hide:
                    return BeatmapsStrings.NsfwExclude;

                case SearchExplicit.Show:
                    return BeatmapsStrings.NsfwInclude;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
