// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// Shows the number of current nominations that a map has received, as well as the number of nominations required for qualification.
    /// </summary>
    public partial class NominationsStatistic : BeatmapCardStatistic
    {
        private NominationsStatistic(int current, int required)
        {
            Icon = FontAwesome.Solid.ThumbsUp;
            Text = current.ToLocalisableString();
            TooltipText = BeatmapsStrings.NominationsRequiredText(current.ToLocalisableString(), required.ToLocalisableString());
        }

        public static NominationsStatistic? CreateFor(APIBeatmapSet beatmapSet)
        {
            // web does not show nominations unless hypes are also present.
            // see: https://github.com/ppy/osu-web/blob/8ed7d071fd1d3eaa7e43cf0e4ff55ca2fef9c07c/resources/assets/lib/beatmapset-panel.tsx#L443
            if (beatmapSet.HypeStatus == null || beatmapSet.NominationStatus == null)
                return null;

            int current = beatmapSet.NominationStatus.Current;
            int requiredMainRuleset = beatmapSet.NominationStatus.RequiredMeta.MainRuleset;
            int requiredNonMainRuleset = beatmapSet.NominationStatus.RequiredMeta.NonMainRuleset;

            int rulesets = beatmapSet.Beatmaps.GroupBy(b => b.Ruleset).Count();

            return new NominationsStatistic(current, requiredMainRuleset + requiredNonMainRuleset * (rulesets - 1));
        }
    }
}
