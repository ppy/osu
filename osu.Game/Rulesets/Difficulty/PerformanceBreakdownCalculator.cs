// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

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

        public async Task<PerformanceBreakdown?> CalculateAsync(ScoreInfo score, CancellationToken cancellationToken = default)
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
                performanceCalculator.GetPerfectPerformanceAsync(playableBeatmap, attributes.Value.Attributes, score.Mods, cancellationToken)
            ).ConfigureAwait(false);

            return new PerformanceBreakdown(performanceArray[0] ?? new PerformanceAttributes(), performanceArray[1] ?? new PerformanceAttributes());
        }
    }
}
