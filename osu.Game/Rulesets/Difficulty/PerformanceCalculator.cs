// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class PerformanceCalculator
    {
        protected readonly IList<TimedDifficultyAttributes> TimedAttributes;

        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;

        protected readonly ScoreInfo Score;

        protected double TimeRate { get; private set; } = 1;

        protected PerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, ScoreInfo score)
        {
            Ruleset = ruleset;

            Score = score;

            beatmap.Mods.Value = score.Mods;
            Beatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo);

            TimedAttributes = ruleset.CreateDifficultyCalculator(beatmap).CalculateTimed(score.Mods).ToList();

            ApplyMods(score.Mods);
        }

        protected virtual void ApplyMods(Mod[] mods)
        {
            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));
            TimeRate = clock.Rate;
        }

        public double Calculate(Dictionary<string, double> categoryDifficulty = null) => Calculate(Double.PositiveInfinity, categoryDifficulty);

        public double Calculate(double time, Dictionary<string, double> categoryDifficulty = null)
        {
            TimedDifficultyAttributes attributes = TimedAttributes.LastOrDefault(a => time >= a.Time) ?? TimedAttributes.First();
            return Calculate(attributes.Attributes, categoryDifficulty);
        }

        protected abstract double Calculate(DifficultyAttributes attributes, Dictionary<string, double> categoryDifficulty = null);
    }
}
