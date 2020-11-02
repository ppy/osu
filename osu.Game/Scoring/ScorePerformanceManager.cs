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

namespace osu.Game.Scoring
{
    /// <summary>
    /// A global component which calculates and caches results of performance calculations for locally databased scores.
    /// </summary>
    public class ScorePerformanceManager : Component
    {
        // this cache will grow indefinitely per session and should be considered temporary.
        // this whole component should likely be replaced with database persistence.
        private readonly ConcurrentDictionary<PerformanceCacheLookup, double> performanceCache = new ConcurrentDictionary<PerformanceCacheLookup, double>();

        [Resolved]
        private BeatmapDifficultyManager difficultyManager { get; set; }

        /// <summary>
        /// Calculates performance for the given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to do the calculation on. </param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        public Task<double?> CalculatePerformanceAsync([NotNull] ScoreInfo score, CancellationToken token = default)
        {
            var lookupKey = new PerformanceCacheLookup(score);

            if (performanceCache.TryGetValue(lookupKey, out double performance))
                return Task.FromResult((double?)performance);

            return computePerformanceAsync(score, lookupKey, token);
        }

        private async Task<double?> computePerformanceAsync(ScoreInfo score, PerformanceCacheLookup lookupKey, CancellationToken token = default)
        {
            var attributes = await difficultyManager.GetDifficultyAsync(score.Beatmap, score.Ruleset, score.Mods, token);

            // Performance calculation requires the beatmap and ruleset to be locally available. If not, return a default value.
            if (attributes.Attributes == null)
                return null;

            token.ThrowIfCancellationRequested();

            var calculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator(attributes.Attributes, score);
            var total = calculator?.Calculate();

            if (total.HasValue)
                performanceCache[lookupKey] = total.Value;

            return total;
        }

        public readonly struct PerformanceCacheLookup
        {
            public readonly string ScoreHash;
            public readonly int LocalScoreID;

            public PerformanceCacheLookup(ScoreInfo info)
            {
                ScoreHash = info.Hash;
                LocalScoreID = info.ID;
            }

            public override int GetHashCode()
            {
                var hash = new HashCode();

                hash.Add(ScoreHash);
                hash.Add(LocalScoreID);

                return hash.ToHashCode();
            }
        }
    }
}
