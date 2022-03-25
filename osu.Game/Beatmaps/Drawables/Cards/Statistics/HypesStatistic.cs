// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// Shows the number of current hypes that a map has received, as well as the number of hypes required for nomination.
    /// </summary>
    public class HypesStatistic : BeatmapCardStatistic
    {
        private HypesStatistic(BeatmapSetHypeStatus hypeStatus)
        {
            Icon = FontAwesome.Solid.Bullhorn;
            Text = hypeStatus.Current.ToLocalisableString();
            TooltipText = BeatmapsStrings.HypeRequiredText(hypeStatus.Current.ToLocalisableString(), hypeStatus.Required.ToLocalisableString());
        }

        public static HypesStatistic? CreateFor(IBeatmapSetOnlineInfo beatmapSetOnlineInfo)
            => beatmapSetOnlineInfo.HypeStatus == null ? null : new HypesStatistic(beatmapSetOnlineInfo.HypeStatus);
    }
}
