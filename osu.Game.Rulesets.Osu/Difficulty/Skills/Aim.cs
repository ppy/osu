// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        public readonly bool IncludeSliders;

        public Aim(Mod[] mods, bool includeSliders)
            : base(mods)
        {
            IncludeSliders = includeSliders;
        }

        private double currentAimStrain;
        private double currentSpeedStrain;

        private double skillMultiplierAim => 26;
        private double skillMultiplierSpeed => 1.3;
        private double meanExponent => 1.25;

        private readonly List<double> sliderStrains = new List<double>();

        private double strainDecayAim(double ms) => Math.Pow(0.15, ms / 1000);
        private double strainDecaySpeed(double ms) => Math.Pow(0.3, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) =>
            DifficultyCalculationUtils.Norm(meanExponent,
                currentAimStrain * strainDecayAim(time - current.Previous(0).StartTime),
                currentSpeedStrain * strainDecaySpeed(time - current.Previous(0).StartTime));

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentAimStrain *= strainDecayAim(current.DeltaTime);
            currentAimStrain += AimEvaluator.EvaluateDifficultyOf(current, IncludeSliders) * skillMultiplierAim;

            currentSpeedStrain *= strainDecaySpeed(current.DeltaTime);
            currentSpeedStrain += SpeedAimEvaluator.EvaluateDifficultyOf(current, Mods) * skillMultiplierSpeed;

            double totalStrain = DifficultyCalculationUtils.Norm(meanExponent, currentAimStrain, currentSpeedStrain);

            if (current.BaseObject is Slider)
                sliderStrains.Add(totalStrain);

            return totalStrain;
        }

        public double GetDifficultSliders()
        {
            if (sliderStrains.Count == 0)
                return 0;

            double maxSliderStrain = sliderStrains.Max();

            if (maxSliderStrain == 0)
                return 0;

            return sliderStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxSliderStrain * 12.0 - 6.0))));
        }

        public double CountTopWeightedSliders(double difficultyValue)
            => OsuStrainUtils.CountTopWeightedSliders(sliderStrains, difficultyValue);
    }
}
