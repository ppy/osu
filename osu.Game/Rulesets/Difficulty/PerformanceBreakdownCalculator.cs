// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Difficulty
{
    public class PerformanceBreakdownCalculator
    {
        private readonly IBeatmap playableBeatmap;
        private readonly BeatmapDifficultyCache difficultyCache;

        public PerformanceBreakdownCalculator(IBeatmap playableBeatmap, BeatmapDifficultyCache difficultyCache)
        {
            this.playableBeatmap = playableBeatmap;
            this.difficultyCache = difficultyCache;
        }

        [ItemCanBeNull]
        public async Task<PerformanceBreakdown> CalculateAsync(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            var attributes = await difficultyCache.GetDifficultyAsync(score.BeatmapInfo!, score.Ruleset, score.Mods, cancellationToken).ConfigureAwait(false);

            var performanceCalculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator();

            // Performance calculation requires the beatmap and ruleset to be locally available. If not, return a default value.
            if (attributes?.Attributes == null || performanceCalculator == null)
                return null;

            cancellationToken.ThrowIfCancellationRequested();

            PerformanceAttributes[] performanceArray = await Task.WhenAll(
                // compute actual performance
                performanceCalculator.CalculateAsync(score, attributes.Value.Attributes, cancellationToken),
                // compute performance for perfect play
                getPerfectPerformance(score, cancellationToken)
            ).ConfigureAwait(false);

            return new PerformanceBreakdown(performanceArray[0] ?? new PerformanceAttributes(), performanceArray[1] ?? new PerformanceAttributes());
        }

        [ItemCanBeNull]
        private Task<PerformanceAttributes> getPerfectPerformance(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                Ruleset ruleset = score.Ruleset.CreateInstance();
                ScoreInfo perfectPlay = ScoreUtils.GetPerfectPlay(playableBeatmap, score.Ruleset, baseScore: score);

                // calculate performance for this perfect score
                var difficulty = await difficultyCache.GetDifficultyAsync(
                    playableBeatmap.BeatmapInfo,
                    score.Ruleset,
                    score.Mods,
                    cancellationToken
                ).ConfigureAwait(false);

                var performanceCalculator = ruleset.CreatePerformanceCalculator();

                if (performanceCalculator == null || difficulty == null)
                    return null;

                return await performanceCalculator.CalculateAsync(perfectPlay, difficulty.Value.Attributes.AsNonNull(), cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }
    }
}
