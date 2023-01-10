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
        private readonly ScorePerformanceCache performanceCache;
        private readonly BeatmapManager beatmapManager;

        public PerformanceBreakdownCalculator(IBeatmap playableBeatmap, ScorePerformanceCache performanceCache, BeatmapManager beatmapManager)
        {
            this.playableBeatmap = playableBeatmap;
            this.performanceCache = performanceCache;
            this.beatmapManager = beatmapManager;
        }

        public async Task<PerformanceBreakdown?> CalculateAsync(ScoreInfo score, CancellationToken cancellationToken = default)
        {
            PerformanceAttributes?[] performanceArray = await Task.WhenAll(
                // compute actual performance
                performanceCache.CalculatePerformanceAsync(score, cancellationToken),
                // compute performance for perfect play
                Task.Run(() => score.Ruleset.CreateInstance().CreatePerformanceCalculator()?.CalculatePerfectPerformance(score, beatmapManager.GetWorkingBeatmap(playableBeatmap.BeatmapInfo)), cancellationToken)
            ).ConfigureAwait(false);

            return new PerformanceBreakdown { Performance = performanceArray[0], PerfectPerformance = performanceArray[1] };
        }
    }
}
