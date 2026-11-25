// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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

        public Task<PerformanceAttributes> CalculateAsync(IScoreInfo score, DifficultyAttributes attributes, CancellationToken cancellationToken)
            => Task.Run(() => CreatePerformanceAttributes(score, attributes), cancellationToken);

        public PerformanceAttributes Calculate(IScoreInfo score, DifficultyAttributes attributes)
            => CreatePerformanceAttributes(score, attributes);

        public PerformanceAttributes Calculate(IScoreInfo score, IWorkingBeatmap beatmap)
        {
            var mods = score.Mods.Select(mod => mod.ToMod(Ruleset));
            return Calculate(score, Ruleset.CreateDifficultyCalculator(beatmap).Calculate(mods));
        }

        /// <summary>
        /// Creates <see cref="PerformanceAttributes"/> to describe a score's performance.
        /// </summary>
        /// <param name="score">The score to create the attributes for.</param>
        /// <param name="attributes">The difficulty attributes for the beatmap relating to the score.</param>
        protected abstract PerformanceAttributes CreatePerformanceAttributes(IScoreInfo score, DifficultyAttributes attributes);
    }
}
