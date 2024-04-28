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
        private readonly List<double> difficulties = new List<double>();
        private double skillMultiplier => 1.26;
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
        protected new double SkillMultiplier => 7.2;

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);

            // We're not using slider aim because we assuming that HD doesn't makes sliders harder (what is not true, but we will ignore this for now)
            double hiddenDifficulty = AimEvaluator.EvaluateDifficultyOf(current, false);
            hiddenDifficulty *= ReadingHiddenEvaluator.EvaluateDifficultyOf(current);
            hiddenDifficulty *= SkillMultiplier;

            CurrentStrain += hiddenDifficulty;

            return CurrentStrain;
        }

        public new static double DifficultyToPerformance(double difficulty) => Math.Max(
            Math.Max(difficulty * 16, Math.Pow(difficulty, 2) * 10), Math.Pow(difficulty, 3) * 4);
    }

    public class ReadingHighAR : GraphSkill
    {

        private const double component_multiplier = 0.135;
        private const double component_default_value_multiplier = 60;
        public ReadingHighAR(Mod[] mods)
            : base(mods)
        {
            aimComponent = new HighARAimComponent(mods);
            speedComponent = new HighARSpeedComponent(mods);
        }

        private HighARAimComponent aimComponent;
        private HighARSpeedComponent speedComponent;

        private readonly List<double> difficulties = new List<double>();
        private int objectsCount = 0;

        public override void Process(DifficultyHitObject current)
        {
            aimComponent.Process(current);
            speedComponent.Process(current);

            if (current.BaseObject is not Spinner)
                objectsCount++;

            double power = OsuDifficultyCalculator.SUM_POWER;
            double mergedDifficulty = Math.Pow(
                Math.Pow(aimComponent.CurrentSectionPeak, power) +
                Math.Pow(speedComponent.CurrentSectionPeak, power), 1.0 / power);

            difficulties.Add(mergedDifficulty);

            if (current.Index == 0)
                CurrentSectionEnd = Math.Ceiling(current.StartTime / SectionLength) * SectionLength;

            while (current.StartTime > CurrentSectionEnd)
            {
                StrainPeaks.Add(CurrentSectionPeak);
                CurrentSectionPeak = 0;
                CurrentSectionEnd += SectionLength;
            }

            CurrentSectionPeak = Math.Max(mergedDifficulty, CurrentSectionPeak);
        }
        public override double DifficultyValue()
        {
            // Simulating summing to get the most correct value possible
            double aimValue = Math.Sqrt(aimComponent.DifficultyValue()) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;
            double speedValue = Math.Sqrt(speedComponent.DifficultyValue()) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;

            double aimPerformance = OsuStrainSkill.DifficultyToPerformance(aimValue);
            double speedPerformance = OsuStrainSkill.DifficultyToPerformance(speedValue);

            double power = OsuDifficultyCalculator.SUM_POWER;
            double totalPerformance = Math.Pow(Math.Pow(aimPerformance, power) + Math.Pow(speedPerformance, power), 1.0 / power);

            // Length bonus is in SR to not inflate Star Rating short AR11 maps
            double lengthBonus = OsuPerformanceCalculator.CalculateDefaultLengthBonus(objectsCount);
            totalPerformance *= Math.Pow(lengthBonus, 4); // make it bypass sqrt

            double adjustedDifficulty = OsuStrainSkill.PerformanceToDifficulty(totalPerformance);
            double difficultyValue = Math.Pow(adjustedDifficulty / OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER, 2.0);

            return 53.2 * Math.Sqrt(difficultyValue);
        }

        public class HighARAimComponent : Aim
        {
            public HighARAimComponent(Mod[] mods)
                : base(mods, true)
            {
            }

            protected new double SkillMultiplier => base.SkillMultiplier * component_multiplier;

            protected override double StrainValueAt(DifficultyHitObject current)
            {
                CurrentStrain *= StrainDecay(current.DeltaTime);

                double aimDifficulty = AimEvaluator.EvaluateDifficultyOf(current, true) * SkillMultiplier;
                aimDifficulty *= ReadingHighAREvaluator.EvaluateDifficultyOf(current, true);

                CurrentStrain += aimDifficulty;

                return CurrentStrain + component_default_value_multiplier * ReadingHighAREvaluator.EvaluateDifficultyOf(current, true);
            }
        }

        public class HighARSpeedComponent : Speed
        {
            public HighARSpeedComponent(Mod[] mods)
                : base(mods)
            {
            }

            protected new double SkillMultiplier => base.SkillMultiplier * component_multiplier;

            protected override double StrainValueAt(DifficultyHitObject current)
            {
                OsuDifficultyHitObject currObj = (OsuDifficultyHitObject)current;

                CurrentStrain *= StrainDecay(currObj.StrainTime);

                double speedDifficulty = SpeedEvaluator.EvaluateDifficultyOf(current) * SkillMultiplier;
                speedDifficulty *= ReadingHighAREvaluator.EvaluateDifficultyOf(current);
                CurrentStrain += speedDifficulty;

                CurrentRhythm = currObj.RhythmDifficulty;
                double totalStrain = CurrentStrain * CurrentRhythm;
                return totalStrain + component_default_value_multiplier * ReadingHighAREvaluator.EvaluateDifficultyOf(current);
            }
        }
    }
}
