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

    public class ReadingLowAR : GraphSkill
    {
        private readonly List<double> difficulties = new List<double>();
        private double skillMultiplier => 1.3;
        private double aimComponentMultiplier => 0.7;
        //private double skillMultiplier => 2;

        public ReadingLowAR(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecayBase => 0.15;
        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        private double currentDensityAimStrain = 0;

        public override void Process(DifficultyHitObject current)
        {
            double densityFactor = Math.Max(0, Math.Pow(ReadingEvaluator.EvaluateDenstityOf(current), 1.5) - 1);
            // double density = Math.Max(0, ReadingEvaluator.EvaluateDenstityOf(current));
            currentDensityAimStrain *= strainDecay(current.DeltaTime);
            currentDensityAimStrain += densityFactor * AimEvaluator.EvaluateDifficultyOf(current, false) * aimComponentMultiplier;

            double densityReadingDifficulty = ReadingEvaluator.EvaluateDifficultyOf(current);
            double totalDensityDifficulty = (currentDensityAimStrain + densityReadingDifficulty) * skillMultiplier;

            difficulties.Add(totalDensityDifficulty);

            if (current.Index == 0)
                CurrentSectionEnd = Math.Ceiling(current.StartTime / SectionLength) * SectionLength;

            while (current.StartTime > CurrentSectionEnd)
            {
                StrainPeaks.Add(CurrentSectionPeak);
                CurrentSectionPeak = 0;
                CurrentSectionEnd += SectionLength;
            }

            CurrentSectionPeak = Math.Max(totalDensityDifficulty, CurrentSectionPeak);
        }

        private double reducedNoteCount => 5;
        private double reducedNoteBaseline => 0.7;
        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Sections with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = difficulties.Where(p => p > 0);

            List<double> values = peaks.OrderByDescending(d => d).ToList();

            for (int i = 0; i < Math.Min(values.Count, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp(i / reducedNoteCount, 0, 1)));
                values[i] *= Interpolation.Lerp(reducedNoteBaseline, 1.0, scale);
            }

            values = values.OrderByDescending(d => d).ToList();

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            for (int i = 0; i < values.Count; i++)
            {
                difficulty += values[i] / (i + 1);
            }

            return difficulty;
        }

        public static double DifficultyToPerformance(double difficulty) => Math.Pow(difficulty, 3) * 10.0;
    }

    public class ReadingHidden : OsuStrainSkill
    {
        public ReadingHidden(Mod[] mods)
            : base(mods)
        {
        }

        private double currentStrain;
        private double skillMultiplier => 5;

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * StrainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= StrainDecay(current.DeltaTime);

            // We're not using slider aim because we assuming that HD doesn't makes sliders harder (what is not true, but we will ignore this for now)
            double hiddenDifficulty = AimEvaluator.EvaluateDifficultyOf(current, false);
            hiddenDifficulty *= ReadingHiddenEvaluator.EvaluateDifficultyOf(current);
            hiddenDifficulty *= skillMultiplier;

            currentStrain += hiddenDifficulty;

            return currentStrain;
        }

        public new static double DifficultyToPerformance(double difficulty) => Math.Pow(difficulty, 2) * 25.0;
    }
}
