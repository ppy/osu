// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Catch.Difficulty.Evaluators;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Aggregation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.05;

        protected override int SectionLength => 750;

        public Movement(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            return MovementEvaluator.EvaluateDifficultyOf(current);
        }

        public override double DifficultyValue()
        {
            var peaks = GetCurrentStrainPeaks().ToList();

            if (ObjectDifficulties.Count == 0)
                return 0;

            (double difficulty, _) = HarmonicSeries.Aggregate(peaks, harmonicScale: 15, decayExponent: 0.7);

            // The shortest maps are believed to be valued too much
            // lengthBonus decreases the value of maps with less than 600x combo at quadratic pace
            // 600x combo is chosen as it is around the standard amount of combo for "TV size" maps at Overdose level
            double lengthBonus = 0.75 + 0.25 * (1.0 - Math.Pow((Math.Min(ObjectDifficulties.Count + 1, 600.0) / 600.0) - 1.0, 2.0));
            difficulty *= lengthBonus;

            return difficulty;
        }
    }
}
