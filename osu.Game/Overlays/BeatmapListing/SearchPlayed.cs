// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchPlayedEnumLocalisationMapper))]
    public enum SearchPlayed
    {
        [Description("任意")]
        Any,
        [Description("玩过")]
        Played,
        [Description("没玩过")]
        Unplayed
    }

    public class SearchPlayedEnumLocalisationMapper : EnumLocalisationMapper<SearchPlayed>
    {
        public override LocalisableString Map(SearchPlayed value)
        {
            switch (value)
            {
                case SearchPlayed.Any:
                    return BeatmapsStrings.PlayedAny;

                case SearchPlayed.Played:
                    return BeatmapsStrings.PlayedPlayed;

                case SearchPlayed.Unplayed:
                    return BeatmapsStrings.PlayedUnplayed;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
