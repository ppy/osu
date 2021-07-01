// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchExtraEnumLocalisationMapper))]
    public enum SearchExtra
    {
        [Description("Has Video")]
        Video,

        [Description("Has Storyboard")]
        Storyboard
    }

    public class SearchExtraEnumLocalisationMapper : EnumLocalisationMapper<SearchExtra>
    {
        public override LocalisableString Map(SearchExtra value)
        {
            switch (value)
            {
                case SearchExtra.Video:
                    return BeatmapsStrings.ExtraVideo;

                case SearchExtra.Storyboard:
                    return BeatmapsStrings.ExtraStoryboard;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
