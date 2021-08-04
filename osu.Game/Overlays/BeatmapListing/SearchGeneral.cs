// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchGeneral
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GeneralRecommended))]
        [Description("推荐难度")]
        Recommended,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GeneralConverts))]
        [Description("包括转谱")]
        Converts,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GeneralFollows))]
        [Description("已关注的谱师")]
        Follows
    }
}
