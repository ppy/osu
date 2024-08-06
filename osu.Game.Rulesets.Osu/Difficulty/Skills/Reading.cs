// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class ReadingHighAR : Skill
    {

        private const double skill_multiplier = 0.13727;
        private const double component_default_value_multiplier = 445;
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
        private double objectsPreemptSum = 0;

        public override void Process(DifficultyHitObject current)
        {
            aimComponent.Process(current);
            speedComponent.Process(current);

            if (current.BaseObject is not Spinner)
            {
                objectsCount++;
                objectsPreemptSum += ((OsuDifficultyHitObject)current).Preempt;
            }
        }
        public override double DifficultyValue()
        {
            // Simulating summing to get the most correct value possible
            double aimValue = Math.Sqrt(aimComponent.DifficultyValue() * skill_multiplier) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;
            double speedValue = Math.Sqrt(speedComponent.DifficultyValue() * skill_multiplier) * OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER;

            double aimPerformance = OsuStrainSkill.DifficultyToPerformance(aimValue);
            double speedPerformance = OsuStrainSkill.DifficultyToPerformance(speedValue);

            double power = OsuDifficultyCalculator.SUM_POWER;
            double totalPerformance = Math.Pow(Math.Pow(aimPerformance, power) + Math.Pow(speedPerformance, power), 1.0 / power);

            // Length bonus is in SR to not inflate Star Rating of short AR11 maps
            double lengthBonus = OsuPerformanceCalculator.CalculateDefaultLengthBonus(objectsCount);

            // Get average preempt of objects
            double averagePreempt = objectsPreemptSum / objectsCount / 1000;

            // Increase length bonus for long maps with very high AR
            // https://www.desmos.com/calculator/wz9wckqgzu
            double lengthBonusPower = 2 + 2 * Math.Pow(0.1, Math.Pow(2.3 * averagePreempt, 8));

            // Be sure that increasing AR won't decrease pp
            if (lengthBonus < 1) lengthBonusPower = 2;

            totalPerformance *= Math.Pow(lengthBonus, lengthBonusPower);

            double adjustedDifficulty = OsuStrainSkill.PerformanceToDifficulty(totalPerformance);
            double difficultyValue = Math.Pow(adjustedDifficulty / OsuDifficultyCalculator.DIFFICULTY_MULTIPLIER, 2.0);

            // Sqrt value to make difficulty depend less on mechanical difficulty
            return 53.2 * Math.Sqrt(difficultyValue);
        }

        public class HighARAimComponent : Aim
        {
            public HighARAimComponent(Mod[] mods)
                : base(mods, true)
            {
            }

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
