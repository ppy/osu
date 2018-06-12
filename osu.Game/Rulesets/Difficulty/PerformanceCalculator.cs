// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class PerformanceCalculator
    {
        private readonly Dictionary<string, object> attributes = new Dictionary<string, object>();
        protected IDictionary<string, object> Attributes => attributes;

        protected readonly IBeatmap Beatmap;
        protected readonly Ruleset Ruleset;
        protected readonly Score Score;

        protected double TimeRate { get; private set; } = 1;

        protected PerformanceCalculator(Ruleset ruleset, IBeatmap beatmap, Score score)
        {
            Beatmap = beatmap;
            Ruleset = ruleset;
            Score = score;

            var diffCalc = ruleset.CreateDifficultyCalculator(beatmap, score.Mods);
            diffCalc.Calculate(attributes);

            ApplyMods(score.Mods);
        }

        protected virtual void ApplyMods(Mod[] mods)
        {
            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));
            TimeRate = clock.Rate;
        }

        /// <summary>
        /// Calculates the performance of the provided score.
        /// Optionally provides additional information relating to the performance of the score.
        /// </summary>
        /// <param name="categoryDifficulty">A provided dictionary in which additional information relating to the performance of the score is placed.
        /// May be null if no additional information is required.</param>
        /// <returns>The total performance rating of the score.</returns>
        public abstract double Calculate(Dictionary<string, object> categoryDifficulty = null);
    }
}
