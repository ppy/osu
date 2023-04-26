// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A class which will recommend the most suitable difficulty for the local user from a beatmap set.
    /// This requires the user to be logged in, as it sources from the user's online profile.
    /// </summary>
    public partial class DifficultyRecommender : Component
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        private readonly Dictionary<string, double> recommendedDifficultyMapping = new Dictionary<string, double>();

        [BackgroundDependencyLoader]
        private void load()
        {
            api.LocalUser.BindValueChanged(_ => populateValues(), true);
        }

        /// <summary>
        /// Find the recommended difficulty from a selection of available difficulties for the current local user.
        /// </summary>
        /// <remarks>
        /// This requires the user to be online for now.
        /// </remarks>
        /// <param name="beatmaps">A collection of beatmaps to select a difficulty from.</param>
        /// <returns>The recommended difficulty, or null if a recommendation could not be provided.</returns>
        [CanBeNull]
        public BeatmapInfo GetRecommendedBeatmap(IEnumerable<BeatmapInfo> beatmaps)
        {
            foreach (string r in orderedRulesets)
            {
                if (!recommendedDifficultyMapping.TryGetValue(r, out double recommendation))
                    continue;

                BeatmapInfo beatmapInfo = beatmaps.Where(b => b.Ruleset.ShortName.Equals(r, StringComparison.Ordinal)).MinBy(b =>
                {
                    double difference = b.StarRating - recommendation;
                    return difference >= 0 ? difference * 2 : difference * -1; // prefer easier over harder
                });

                if (beatmapInfo != null)
                    return beatmapInfo;
            }

            return null;
        }

        private void populateValues()
        {
            if (api.LocalUser.Value.RulesetsStatistics == null)
                return;

            foreach (var kvp in api.LocalUser.Value.RulesetsStatistics)
            {
                // algorithm taken from https://github.com/ppy/osu-web/blob/e6e2825516449e3d0f3f5e1852c6bdd3428c3437/app/Models/User.php#L1505
                recommendedDifficultyMapping[kvp.Key] = Math.Pow((double)(kvp.Value.PP ?? 0), 0.4) * 0.195;
            }
        }

        /// <returns>
        /// Rulesets ordered descending by their respective recommended difficulties.
        /// The currently selected ruleset will always be first.
        /// </returns>
        private IEnumerable<string> orderedRulesets
        {
            get
            {
                if (LoadState < LoadState.Ready || ruleset.Value == null)
                    return Enumerable.Empty<string>();

                return recommendedDifficultyMapping
                       .OrderByDescending(pair => pair.Value)
                       .Select(pair => pair.Key)
                       .Where(r => !r.Equals(ruleset.Value.ShortName, StringComparison.Ordinal))
                       .Prepend(ruleset.Value.ShortName);
            }
        }
    }
}
