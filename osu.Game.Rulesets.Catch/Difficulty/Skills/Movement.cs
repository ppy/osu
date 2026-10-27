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

            // Difficulty reduction based on time: the earlier strain is, the easier it is to catch
            const double reduced_difficulty_sections = 40.0; // a total of 40 sections (equivalently: first 45 seconds) are affected
            const double reduced_difficulty_base_line = 0.5;
            const double reduced_difficulty_power = 0.5;

            for (int sectionCount = 0; sectionCount < Math.Min(reduced_difficulty_sections, peaks.Count); sectionCount++)
            {
                double scale = Math.Pow(Math.Min(1.0, sectionCount / reduced_difficulty_sections), reduced_difficulty_power);
                peaks[sectionCount] *= reduced_difficulty_base_line + (1.0 - reduced_difficulty_base_line) * scale;
            }

            (double difficulty, _) = HarmonicSeries.Aggregate(peaks, harmonicScale: 15, decayExponent: 0.7);

            return difficulty;
        }
    }
}
