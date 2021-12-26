// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Scoring
{
    /// <summary>
    /// A component which performs and acts as a central cache for performance calculations of locally databased scores.
    /// Currently not persisted between game sessions.
    /// </summary>
    public class ScorePerformanceCache : MemoryCachingComponent<ScorePerformanceCache.PerformanceCacheLookup, double?>
    {
        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        protected override bool CacheNullValues => false;

        /// <summary>
        /// Calculates performance for the given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to do the calculation on. </param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        public Task<double?> CalculatePerformanceAsync([NotNull] ScoreInfo score, CancellationToken token = default) =>
            GetAsync(new PerformanceCacheLookup(score), token);

        protected override async Task<double?> ComputeValueAsync(PerformanceCacheLookup lookup, CancellationToken token = default)
        {
            var score = lookup.ScoreInfo;

            var attributes = await difficultyCache.GetDifficultyAsync(score.BeatmapInfo, score.Ruleset, score.Mods, token).ConfigureAwait(false);

            // Performance calculation requires the beatmap and ruleset to be locally available. If not, return a default value.
            if (attributes?.Attributes == null)
                return null;

            token.ThrowIfCancellationRequested();

            var calculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator(attributes.Value.Attributes, score);

            return calculator?.Calculate().Total;
        }

        public readonly struct PerformanceCacheLookup
        {
            public readonly ScoreInfo ScoreInfo;

            public PerformanceCacheLookup(ScoreInfo info)
            {
                ScoreInfo = info;
            }

            public override int GetHashCode()
            {
                var hash = new HashCode();

                hash.Add(ScoreInfo.Hash);
                hash.Add(ScoreInfo.ID);

                return hash.ToHashCode();
            }
        }
    }
}
