// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Scoring
{
    public class ScorePerformanceManager : Component
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        /// <summary>
        /// Calculates performance for the given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to do the calculation on. </param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        public async Task<double> CalculatePerformanceAsync([NotNull] ScoreInfo score, CancellationToken token = default)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (token.IsCancellationRequested)
                    return default;

                var beatmap = beatmapManager.GetWorkingBeatmap(score.Beatmap);

                var calculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator(beatmap, score);
                var total = calculator.Calculate();

                return total;
            }, token);
        }
    }
}
