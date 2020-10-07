// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Scoring
{
    public class ScorePerformanceManager : Component
    {
        private readonly ConcurrentDictionary<PerformanceCacheLookup, double> performanceCache = new ConcurrentDictionary<PerformanceCacheLookup, double>();

        [Resolved]
        private BeatmapDifficultyManager difficultyManager { get; set; }

        /// <summary>
        /// Calculates performance for the given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to do the calculation on. </param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        public async Task<double> CalculatePerformanceAsync([NotNull] ScoreInfo score, CancellationToken token = default)
        {
            if (tryGetExisting(score, out var perf, out var lookupKey))
                return perf;

            return await computePerformanceAsync(score, lookupKey, token);
        }

        private bool tryGetExisting(ScoreInfo score, out double performance, out PerformanceCacheLookup lookupKey)
        {
            lookupKey = new PerformanceCacheLookup(score);

            return performanceCache.TryGetValue(lookupKey, out performance);
        }

        private async Task<double> computePerformanceAsync(ScoreInfo score, PerformanceCacheLookup lookupKey, CancellationToken token = default)
        {
            var attributes = await difficultyManager.GetDifficultyAsync(score.Beatmap, score.Ruleset, score.Mods, token);

            if (token.IsCancellationRequested)
                return default;

            var calculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator(attributes.Attributes, score);
            var total = calculator?.Calculate() ?? default;

            performanceCache[lookupKey] = total;

            return total;
        }

        public readonly struct PerformanceCacheLookup
        {
            public readonly double Accuracy;
            public readonly int BeatmapId;
            public readonly long TotalScore;
            public readonly int Combo;
            public readonly Mod[] Mods;
            public readonly int RulesetId;

            public PerformanceCacheLookup(ScoreInfo info)
            {
                Accuracy = info.Accuracy;
                BeatmapId = info.Beatmap.ID;
                TotalScore = info.TotalScore;
                Combo = info.Combo;
                Mods = info.Mods;
                RulesetId = info.Ruleset.ID ?? 0;
            }

            public override int GetHashCode()
            {
                var hash = new HashCode();

                hash.Add(Accuracy);
                hash.Add(BeatmapId);
                hash.Add(TotalScore);
                hash.Add(Combo);
                hash.Add(RulesetId);
                foreach (var mod in Mods)
                    hash.Add(mod);

                return hash.ToHashCode();
            }
        }
    }
}
