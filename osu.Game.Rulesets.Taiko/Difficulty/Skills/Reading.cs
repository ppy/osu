// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the reading coefficient of taiko difficulty.
    /// </summary>
    public class Reading : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.4;

        private double currentPatternLength;
        private double currentPatternDifficultySum;
        public double WeightedTotalDifficultySum;

        private double currentStrain;
        private readonly Mod[] mods;

        public Reading(Mod[] mods)
            : base(mods)
        {
            this.mods = mods;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Drum Rolls and Swells are exempt.
            if (current.BaseObject is not Hit)
            {
                return 0.0;
            }

            var taikoObject = (TaikoDifficultyHitObject)current;
            var colourData = taikoObject.ColourData;

            int index = colourData.MonoStreak?.HitObjects.IndexOf(taikoObject) ?? 0;

            currentStrain *= DifficultyCalculationUtils.Logistic(index, 4, -1 / 25.0, 0.5) + 0.5;

            double difficulty = ReadingEvaluator.EvaluateDifficultyOf(taikoObject, mods);

            currentStrain *= StrainDecayBase;
            currentStrain += difficulty * SkillMultiplier;

            bool isNewPattern = colourData.RepeatingHitPattern?.FirstHitObject == taikoObject;

            // Add the average reading difficulty of the previous pattern to the sum when a new pattern is started.
            if (isNewPattern)
            {
                WeightedTotalDifficultySum += currentPatternDifficultySum / Math.Max(currentPatternLength, 1);

                currentPatternLength = 0;
                currentPatternDifficultySum = 0;
            }

            currentPatternLength++;
            currentPatternDifficultySum += difficulty;

            return currentStrain;
        }
    }
}
