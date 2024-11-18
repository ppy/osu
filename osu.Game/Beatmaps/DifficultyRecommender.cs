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
using osu.Game.Online;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A class which will recommend the most suitable difficulty for the local user from a beatmap set.
    /// This requires the user to be logged in, as it sources from the user's online profile.
    /// </summary>
    public partial class DifficultyRecommender : Component
    {
        private readonly LocalUserStatisticsProvider statisticsProvider;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        private readonly Dictionary<string, double> recommendedDifficultyMapping = new Dictionary<string, double>();

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

        private IBindable<UserStatisticsUpdate> statisticsUpdate = null!;

        public DifficultyRecommender(LocalUserStatisticsProvider statisticsProvider)
        {
            this.statisticsProvider = statisticsProvider;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            statisticsUpdate = statisticsProvider.StatisticsUpdate.GetBoundCopy();
            statisticsUpdate.BindValueChanged(u =>
            {
                if (u.NewValue == null)
                    return;

                // algorithm taken from https://github.com/ppy/osu-web/blob/e6e2825516449e3d0f3f5e1852c6bdd3428c3437/app/Models/User.php#L1505
                recommendedDifficultyMapping[u.NewValue.Ruleset.ShortName] = Math.Pow((double)(u.NewValue.NewStatistics.PP ?? 0), 0.4) * 0.195;
            }, true);
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
    }
}
