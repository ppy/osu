// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Scoring
{
    public abstract class PerformanceCalculator
    {
        private readonly Dictionary<string, double> attributes = new Dictionary<string, double>();
        protected IDictionary<string, double> Attributes => attributes;

        protected readonly IBeatmap Beatmap;
        protected readonly Score Score;

        protected PerformanceCalculator(Ruleset ruleset, IBeatmap beatmap, Score score)
        {
            Score = score;

            Beatmap = beatmap;

            var diffCalc = ruleset.CreateDifficultyCalculator(beatmap, score.Mods);
            diffCalc.Calculate(attributes);
        }

        public abstract double Calculate(Dictionary<string, double> categoryDifficulty = null);
    }
}
