// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : Skill
    {
        private readonly List<double> difficulties = new List<double>();
        private const double skill_multiplier = 2.4;

        public Reading(Mod[] mods)
            : base(mods)
        {
        }

        public override void Process(DifficultyHitObject current) => difficulties.Add(ReadingEvaluator.EvaluateDifficultyOf(current, Mods) * skill_multiplier);

        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Sections with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = difficulties.Where(p => p > 0);

            List<double> values = peaks.OrderByDescending(d => d).ToList();

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            for (int i = 0; i < values.Count; i++)
            {
                difficulty += values[i] / (i + 1);
            }

            return difficulty;
        }
    }
}
