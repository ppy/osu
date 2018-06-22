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
        protected readonly DifficultyAttributes Attributes;

        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;
        protected readonly Score Score;

        protected double TimeRate { get; private set; } = 1;

        protected PerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, Score score)
        {
            Ruleset = ruleset;
            Score = score;

            beatmap.Mods.Value = score.Mods;
            Beatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo);

            Attributes = ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods);

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
