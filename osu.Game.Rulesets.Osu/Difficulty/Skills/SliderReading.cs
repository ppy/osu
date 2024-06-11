// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class SliderReading : OsuStrainSkill
    {
        public SliderReading(Mod[] mods)
            : base(mods)
        {
        }


        private double currentStrain;

        private double skillMultiplier => 30;
        private double strainDecayBase => 0.15;

        private List<OsuDifficultyHitObject> previousSliders = [];

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            double difficulty = SliderReadingEvaluator.EvaluateDifficultyOf(current, previousSliders) * skillMultiplier;

            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += difficulty;

            if (current.BaseObject is Slider) previousSliders.Add((OsuDifficultyHitObject)current);

            return currentStrain + difficulty;
        }

        public static double DifficultyToPerformance(double sliderDifficulty, double aimDifficulty)
        {
            double sliderPerformance = 4.49 * Math.Pow(sliderDifficulty, 3);
            double aimPerformance = Math.Pow(5 * Math.Max(1, aimDifficulty / 0.0675) - 4, 3) / 100000;
            return Math.Max(0, sliderPerformance - aimPerformance / 10);
        }
    }
}
