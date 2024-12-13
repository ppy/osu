// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A class which will recommend the most suitable difficulty for the local user from a beatmap set.
    /// This requires the user to be logged in, as it sources from the user's online profile.
    /// </summary>
    public partial class DifficultyRecommender : Component
    {
        public event Action? StarRatingUpdated;

        private readonly LocalUserStatisticsProvider statisticsProvider;

        [Resolved]
        private Bindable<RulesetInfo> gameRuleset { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private readonly Dictionary<string, double> recommendedDifficultyMapping = new Dictionary<string, double>();

        /// <returns>
        /// Rulesets ordered descending by their respective recommended difficulties.
        /// The currently selected ruleset will always be first.
        /// </returns>
        private IEnumerable<string> orderedRulesets
        {
            get
            {
                if (LoadState < LoadState.Ready || gameRuleset.Value == null)
                    return Enumerable.Empty<string>();

                return recommendedDifficultyMapping
                       .OrderByDescending(pair => pair.Value)
                       .Select(pair => pair.Key)
                       .Where(r => !r.Equals(gameRuleset.Value.ShortName, StringComparison.Ordinal))
                       .Prepend(gameRuleset.Value.ShortName);
            }
        }

        public DifficultyRecommender(LocalUserStatisticsProvider statisticsProvider)
        {
            this.statisticsProvider = statisticsProvider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var ruleset in rulesets.AvailableRulesets)
            {
                if (statisticsProvider.GetStatisticsFor(ruleset) is UserStatistics statistics)
                    updateMapping(ruleset, statistics);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            statisticsProvider.StatisticsUpdated += onStatisticsUpdated;
        }

        private void onStatisticsUpdated(UserStatisticsUpdate update) => updateMapping(update.Ruleset, update.NewStatistics);

        private void updateMapping(RulesetInfo ruleset, UserStatistics statistics)
        {
            // algorithm taken from https://github.com/ppy/osu-web/blob/027026fccc91525e39cee5d2f369f1b343eb1bf1/app/Models/UserStatistics/Model.php#L93-L94
            recommendedDifficultyMapping[ruleset.ShortName] =
                ruleset.ShortName == @"taiko"
                    ? Math.Pow((double)(statistics.PP ?? 0), 0.35) * 0.27
                    : Math.Pow((double)(statistics.PP ?? 0), 0.4) * 0.195;

            StarRatingUpdated?.Invoke();
        }

        public double? GetRecommendedStarRatingFor(RulesetInfo ruleset)
            => recommendedDifficultyMapping.TryGetValue(ruleset.ShortName, out double starRating) ? starRating : null;

        /// <summary>
        /// Find the recommended difficulty from a selection of available difficulties for the current local user.
        /// </summary>
        /// <remarks>
        /// This requires the user to be online for now.
        /// </remarks>
        /// <param name="beatmaps">A collection of beatmaps to select a difficulty from.</param>
        /// <returns>The recommended difficulty, or null if a recommendation could not be provided.</returns>
        public BeatmapInfo? GetRecommendedBeatmap(IEnumerable<BeatmapInfo> beatmaps)
        {
            foreach (string r in orderedRulesets)
            {
                if (!recommendedDifficultyMapping.TryGetValue(r, out double recommendation))
                    continue;

                BeatmapInfo? beatmapInfo = beatmaps.Where(b => b.Ruleset.ShortName.Equals(r, StringComparison.Ordinal)).MinBy(b =>
                {
                    double difference = b.StarRating - recommendation;
                    return difference >= 0 ? difference * 2 : difference * -1; // prefer easier over harder
                });

                if (beatmapInfo != null)
                    return beatmapInfo;
            }

            return null;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (statisticsProvider.IsNotNull())
                statisticsProvider.StatisticsUpdated -= onStatisticsUpdated;

            base.Dispose(isDisposing);
        }
    }
}
