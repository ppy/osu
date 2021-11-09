// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        public NominationsStatistic(BeatmapSetNominationStatus nominationStatus)
        {
            Icon = FontAwesome.Solid.ThumbsUp;
            Text = nominationStatus.Current.ToLocalisableString();
            TooltipText = BeatmapsStrings.NominationsRequiredText(nominationStatus.Current.ToLocalisableString(), nominationStatus.Required.ToLocalisableString());
        }
    }
}
