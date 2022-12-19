﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Scoring
{
    /// <summary>
    /// A component which performs and acts as a central cache for performance calculations of locally databased scores.
    /// Currently not persisted between game sessions.
    /// </summary>
    public partial class ScorePerformanceCache : MemoryCachingComponent<ScorePerformanceCache.PerformanceCacheLookup, PerformanceAttributes>
    {
        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        protected override bool CacheNullValues => false;

        /// <summary>
        /// Calculates performance for the given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to do the calculation on. </param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        public Task<PerformanceAttributes> CalculatePerformanceAsync([NotNull] ScoreInfo score, CancellationToken token = default) =>
            GetAsync(new PerformanceCacheLookup(score), token);

        protected override async Task<PerformanceAttributes> ComputeValueAsync(PerformanceCacheLookup lookup, CancellationToken token = default)
        {
            var score = lookup.ScoreInfo;

            var attributes = await difficultyCache.GetDifficultyAsync(score.BeatmapInfo, score.Ruleset, score.Mods, token).ConfigureAwait(false);

            // Performance calculation requires the beatmap and ruleset to be locally available. If not, return a default value.
            if (attributes?.Attributes == null)
                return null;

            token.ThrowIfCancellationRequested();

            return score.Ruleset.CreateInstance().CreatePerformanceCalculator()?.Calculate(score, attributes.Value.Attributes);
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
