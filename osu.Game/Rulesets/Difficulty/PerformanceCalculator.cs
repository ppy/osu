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
        private readonly Dictionary<string, double> attributes = new Dictionary<string, double>();
        protected IDictionary<string, double> Attributes => attributes;

        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;
        protected readonly Score Score;

        protected double TimeRate { get; private set; } = 1;

        protected PerformanceCalculator(Ruleset ruleset, IBeatmap beatmap, Score score)
        {
            Ruleset = ruleset;
            Beatmap = beatmap;
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

        public abstract double Calculate(Dictionary<string, double> categoryDifficulty = null);
    }
}
