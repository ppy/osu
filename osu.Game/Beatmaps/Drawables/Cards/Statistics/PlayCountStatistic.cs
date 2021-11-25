// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// Shows the number of times the given beatmap set has been played.
    /// </summary>
    public class PlayCountStatistic : BeatmapCardStatistic
    {
        public PlayCountStatistic(IBeatmapSetOnlineInfo onlineInfo)
        {
            Icon = FontAwesome.Regular.PlayCircle;
            Text = onlineInfo.PlayCount.ToMetric(decimals: 1);
            TooltipText = BeatmapsStrings.PanelPlaycount(onlineInfo.PlayCount.ToLocalisableString(@"N0"));
        }
    }
}
