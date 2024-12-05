// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Users;
using static osu.Game.Screens.Select.Carousel.DrawableCarouselBeatmap;

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
        private Bindable<RulesetInfo> gameRuleset { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private readonly Dictionary<string, double> userPerformanceStats = new Dictionary<string, double>();

        /// <returns>
        /// Rulesets ordered descending by player skill level in them.
        /// The currently selected ruleset will always be first.
        /// </returns>
        private IEnumerable<string> orderedRulesets
        {
            get
            {
                if (LoadState < LoadState.Ready || gameRuleset.Value == null)
                    return Enumerable.Empty<string>();

                return userPerformanceStats
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
            userPerformanceStats[ruleset.ShortName] = (double)(statistics.PP ?? 0);
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
                if (!userPerformanceStats.TryGetValue(r, out double userPerformance))
                    continue;

                // algorithm taken from https://github.com/ppy/osu-web/blob/e6e2825516449e3d0f3f5e1852c6bdd3428c3437/app/Models/User.php#L1505
                double recommendation = Math.Pow(userPerformance, 0.4) * 0.195;

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

        public void ScheduleRecommendedHighlight(IEnumerable<DrawableCarouselItem> beatmaps)
        {
            int beatmapCount = beatmaps.Count();

            if (beatmapCount == 0)
                return;

            var difficulties = new Dictionary<DrawableCarouselBeatmap, StarDifficulty?>();

            foreach (DrawableCarouselItem beatmap in beatmaps)
                ((DrawableCarouselBeatmap)beatmap).StarDifficultyChangedCallback = (d) => updateRecommendedHighlight((DrawableCarouselBeatmap)beatmap, d);

            void updateRecommendedHighlight(DrawableCarouselBeatmap updatedBeatmap, StarDifficulty? updatedDifficulty)
            {
                difficulties[updatedBeatmap] = updatedDifficulty;

                // Check if all are calculated
                if (difficulties.Count < beatmapCount)
                    return;

                if (!userPerformanceStats.TryGetValue(gameRuleset.Value.ShortName, out double userPerformance))
                    return;

                // https://www.desmos.com/calculator/vob1fblngp
                double higherBound = Math.Pow(userPerformance, 0.92) * 0.18 + 15;
                double target = Math.Pow(userPerformance, 0.9) * 0.17;
                double lowerBound = Math.Max(Math.Pow(userPerformance, 0.9) * 0.12 - 5, 0);

                DrawableCarouselBeatmap bestMatch = null;
                double bestMatchDelta = double.PositiveInfinity;
                RecommendationType bestMatchType = RecommendationType.NotRecommended;

                foreach ((DrawableCarouselBeatmap beatmap, StarDifficulty? difficulty) in difficulties)
                {
                    if (difficulty == null || difficulty.Value.PerformanceAttributes == null)
                        continue;

                    double mapPerformance = difficulty.Value.PerformanceAttributes!.Total;
                    double delta = Math.Abs(mapPerformance - target);

                    // prefer harder over easier accounting for the fact that this is SS value
                    if (mapPerformance < target) delta *= 2;

                    if (delta < bestMatchDelta)
                    {
                        bestMatch = beatmap;
                        bestMatchDelta = delta;

                        if (mapPerformance >= lowerBound && mapPerformance <= higherBound)
                            bestMatchType = RecommendationType.Recommended;
                        else if (mapPerformance <= higherBound)
                            bestMatchType = RecommendationType.TooEasy;
                        else if (mapPerformance >= lowerBound)
                            bestMatchType = RecommendationType.TooHard;
                    }
                }

                foreach ((DrawableCarouselBeatmap beatmap, _) in difficulties)
                    beatmap.RecommendedType = beatmap == bestMatch ? bestMatchType : RecommendationType.NotRecommended;

            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (statisticsProvider.IsNotNull())
                statisticsProvider.StatisticsUpdated -= onStatisticsUpdated;

            base.Dispose(isDisposing);
        }
    }
}
