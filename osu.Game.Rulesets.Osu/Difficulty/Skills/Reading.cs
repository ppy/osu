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
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class ReadingLowAR : GraphSkill
    {
        private double skillMultiplier => 1.22;
        private double aimComponentMultiplier => 0.4;

        public ReadingLowAR(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecayBase => 0.15;
        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        private double currentDensityAimStrain = 0;

        public override void Process(DifficultyHitObject current)
        {
            double densityReadingDifficulty = ReadingEvaluator.EvaluateDifficultyOf(current);
            double densityAimingFactor = ReadingEvaluator.EvaluateAimingDensityFactorOf(current);

            currentDensityAimStrain *= strainDecay(current.DeltaTime);
            currentDensityAimStrain += densityAimingFactor * AimEvaluator.EvaluateDifficultyOf(current, true) * aimComponentMultiplier;

            double totalDensityDifficulty = (currentDensityAimStrain + densityReadingDifficulty) * skillMultiplier;

            ObjectStrains.Add(totalDensityDifficulty);

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

    public class ReadingHidden : Aim
    {
        public ReadingHidden(Mod[] mods)
            : base(mods, false)
        {
        }
        protected new double SkillMultiplier => 7.3;

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);

            // We're not using slider aim because we assuming that HD doesn't makes sliders harder (what is not true, but we will ignore this for now)
            double hiddenDifficulty = AimEvaluator.EvaluateDifficultyOf(current, false);
            hiddenDifficulty *= ReadingHiddenEvaluator.EvaluateDifficultyOf(current);
            hiddenDifficulty *= SkillMultiplier;

            CurrentStrain += hiddenDifficulty;
            ObjectStrains.Add(CurrentStrain);

            return CurrentStrain;
        }

        public new static double DifficultyToPerformance(double difficulty) => Math.Max(
            Math.Max(difficulty * 16, Math.Pow(difficulty, 2) * 10), Math.Pow(difficulty, 3) * 4);
    }

    public class ReadingHighAR : GraphSkill
    {
        public const double MECHANICAL_PP_POWER = 0.6;
        private const double skill_multiplier = 9.31;
        private const double component_default_value_multiplier = 280;
        public ReadingHighAR(Mod[] mods)
            : base(mods)
        {
            aimComponent = new HighARAimComponent(mods);
            speedComponent = new HighARSpeedComponent(mods);
        }

        private HighARAimComponent aimComponent;
        private HighARSpeedComponent speedComponent;

        public override void Process(DifficultyHitObject current)
        {
            aimComponent.Process(current);
            speedComponent.Process(current);

            if (current.Index == 0)
                CurrentSectionEnd = Math.Ceiling(current.StartTime / SectionLength) * SectionLength;

            while (current.StartTime > CurrentSectionEnd)
            {
                StrainPeaks.Add(CurrentSectionPeak);
                CurrentSectionPeak = 0;
                CurrentSectionEnd += SectionLength;
            }

            double visualDifficultyValue = scaleDifficulty(aimComponent.CurrentSectionPeak, speedComponent.CurrentSectionPeak);
            CurrentSectionPeak = Math.Max(visualDifficultyValue, CurrentSectionPeak);
        }

        // Coefs for curve similar to difficulty to performance curve
        private static double power => 3;
        private static double multiplier => 3.7;

        public static double DifficultyToPerformance(double difficulty) => Math.Pow(difficulty, power) * multiplier;
        private static double performanceToDifficulty(double performance) => Math.Pow(performance / multiplier, 1.0 / power);

        private static double scaleDifficulty(double aimPart, double speedPart)
        {
            // Simulating summing to get the most correct value possible
            double aimValue = Math.Sqrt(aimPart * skill_multiplier) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;
            double speedValue = Math.Sqrt(speedPart * skill_multiplier) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;

            double aimPerformance = DifficultyToPerformance(aimValue);
            double speedPerformance = DifficultyToPerformance(speedValue);

            double sumPower = OsuDifficultyCalculator.SUM_POWER;
            double totalPerformance = Math.Pow(Math.Pow(aimPerformance, sumPower) + Math.Pow(speedPerformance, sumPower), 1.0 / sumPower);

            double newSkillValue = performanceToDifficulty(totalPerformance);
            double difficultyValue = Math.Pow(newSkillValue / OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER, 2.0);

            difficultyValue = Math.Pow(difficultyValue / skill_multiplier, MECHANICAL_PP_POWER);

            return difficultyValue;
        }

        public override double DifficultyValue() => skill_multiplier * scaleDifficulty(aimComponent.DifficultyValue(), speedComponent.DifficultyValue());
        public class HighARAimComponent : Aim
        {
            public HighARAimComponent(Mod[] mods)
                : base(mods, true)
            {
            }

            protected override double StrainValueAt(DifficultyHitObject current)
            {
                CurrentStrain *= StrainDecay(current.DeltaTime);

                double highARDifficulty = Math.Pow(ReadingHighAREvaluator.EvaluateDifficultyOf(current, true), 1.0 / MECHANICAL_PP_POWER);
                double aimDifficulty = AimEvaluator.EvaluateDifficultyOf(current, true) * SkillMultiplier;

                aimDifficulty *= highARDifficulty;
                CurrentStrain += aimDifficulty;

                return CurrentStrain + component_default_value_multiplier * highARDifficulty;
            }
        }

        public class HighARSpeedComponent : Speed
        {
            public HighARSpeedComponent(Mod[] mods)
                : base(mods)
            {
            }

            protected override double StrainValueAt(DifficultyHitObject current)
            {
                OsuDifficultyHitObject currObj = (OsuDifficultyHitObject)current;

                CurrentStrain *= StrainDecay(currObj.StrainTime);

                double highARDifficulty = Math.Pow(ReadingHighAREvaluator.EvaluateDifficultyOf(current, false), 1.0 / MECHANICAL_PP_POWER);
                double speedDifficulty = SpeedEvaluator.EvaluateDifficultyOf(current) * SkillMultiplier;

                speedDifficulty *= highARDifficulty;
                CurrentStrain += speedDifficulty;

                CurrentRhythm = currObj.RhythmDifficulty;
                double totalStrain = CurrentStrain * CurrentRhythm;
                return totalStrain + component_default_value_multiplier * highARDifficulty;
            }
        }
    }
}
