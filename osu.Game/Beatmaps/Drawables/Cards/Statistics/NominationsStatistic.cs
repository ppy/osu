// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// Shows the number of current nominations that a map has received, as well as the number of nominations required for qualification.
    /// </summary>
    public class NominationsStatistic : BeatmapCardStatistic
    {
        private NominationsStatistic(BeatmapSetNominationStatus nominationStatus)
        {
            Icon = FontAwesome.Solid.ThumbsUp;
            Text = nominationStatus.Current.ToLocalisableString();
            TooltipText = BeatmapsStrings.NominationsRequiredText(nominationStatus.Current.ToLocalisableString(), nominationStatus.Required.ToLocalisableString());
        }

        public static NominationsStatistic? CreateFor(IBeatmapSetOnlineInfo beatmapSetOnlineInfo)
            // web does not show nominations unless hypes are also present.
            // see: https://github.com/ppy/osu-web/blob/8ed7d071fd1d3eaa7e43cf0e4ff55ca2fef9c07c/resources/assets/lib/beatmapset-panel.tsx#L443
            => beatmapSetOnlineInfo.HypeStatus == null || beatmapSetOnlineInfo.NominationStatus == null ? null : new NominationsStatistic(beatmapSetOnlineInfo.NominationStatus);
    }
}
