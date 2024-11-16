// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class PerformanceCalculator
    {
        protected readonly Ruleset Ruleset;

        protected PerformanceCalculator(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        public Task<IPerformanceAttributes> CalculateAsync(ScoreInfo score, IDifficultyAttributes attributes, CancellationToken cancellationToken)
            => Task.Run(() => CreatePerformanceAttributes(score, attributes), cancellationToken);

        public IPerformanceAttributes Calculate(ScoreInfo score, IDifficultyAttributes attributes)
            => CreatePerformanceAttributes(score, attributes);

        public IPerformanceAttributes Calculate(ScoreInfo score, IWorkingBeatmap beatmap)
            => Calculate(score, Ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods));

        /// <summary>
        /// Creates <see cref="IPerformanceAttributes"/> to describe a score's performance.
        /// </summary>
        /// <param name="score">The score to create the attributes for.</param>
        /// <param name="attributes">The difficulty attributes for the beatmap relating to the score.</param>
        protected abstract IPerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, IDifficultyAttributes attributes);
    }
}
