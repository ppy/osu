// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class ReadingLowAr : StrainSkill
    {
        private double skillMultiplier => 1.22;
        private double aimComponentMultiplier => 0.4;

        public ReadingLowAr(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecayBase => 0.15;
        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        private double currentDensityAimStrain;

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            double densityReadingDifficulty = ReadingEvaluator.EvaluateDifficultyOf(current);
            double densityAimingFactor = ReadingEvaluator.EvaluateAimingDensityFactorOf(current);

            // Reward slideraim but not bigger than 2 * sliderless aim
            double aimDifficulty = Math.Min(AimEvaluator.EvaluateDifficultyOf(current, true), 2 * AimEvaluator.EvaluateDifficultyOf(current, true));

            currentDensityAimStrain *= strainDecay(current.DeltaTime);
            currentDensityAimStrain += densityAimingFactor * aimDifficulty * aimComponentMultiplier;

            double totalDensityDifficulty = (currentDensityAimStrain + densityReadingDifficulty) * skillMultiplier;
            return totalDensityDifficulty;
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => 0;

        private double reducedNoteCount => 5;
        private double reducedNoteBaseline => 0.7;

        public override double DifficultyValue()
        {
            // Sections with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = ObjectStrains.Where(p => p > 0);

            List<double> values = peaks.OrderByDescending(d => d).ToList();

            for (int i = 0; i < Math.Min(values.Count, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp(i / reducedNoteCount, 0, 1)));
                values[i] *= Interpolation.Lerp(reducedNoteBaseline, 1.0, scale);
            }

            values = values.OrderByDescending(d => d).ToList();

            double difficulty = 0;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            for (int i = 0; i < values.Count; i++)
            {
                difficulty += values[i] / (i + 1);
            }

            return difficulty;
        }

        public static double DifficultyToPerformance(double difficulty) => Math.Max(
            Math.Max(Math.Pow(difficulty, 1.5) * 20, Math.Pow(difficulty, 2) * 17.0),
            Math.Max(Math.Pow(difficulty, 3) * 10.5, Math.Pow(difficulty, 4) * 6.00));
    }
}
