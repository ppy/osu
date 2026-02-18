// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Mods;
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

        private double skillMultiplierAim => 25.85;
        private double skillMultiplierSpeed => 1.35;
        private double skillMultiplierTotal => 1.0;
        private double meanExponent => 1.2;

        private readonly List<double> sliderStrains = new List<double>();

        private double strainDecayAim(double ms) => Math.Pow(0.15, ms / 1000);
        private double strainDecaySpeed(double ms) => Math.Pow(0.3, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) =>
            DifficultyCalculationUtils.Norm(meanExponent,
                currentAimStrain * strainDecayAim(time - current.Previous(0).StartTime),
                currentSpeedStrain * strainDecaySpeed(time - current.Previous(0).StartTime));

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            double decayAim = strainDecayAim(((OsuDifficultyHitObject)current).AdjustedDeltaTime);
            double decaySpeed = strainDecaySpeed(((OsuDifficultyHitObject)current).AdjustedDeltaTime);

            double aimDifficulty = AimEvaluator.EvaluateDifficultyOf(current, IncludeSliders);
            double speedDifficulty = SpeedAimEvaluator.EvaluateDifficultyOf(current);

            if (Mods.Any(m => m is OsuModTouchDevice))
            {
                aimDifficulty = Math.Pow(aimDifficulty, 0.8);
                speedDifficulty = Math.Pow(speedDifficulty, 0.95);
            }

            currentAimStrain *= decayAim;
            currentAimStrain += aimDifficulty * (1 - decayAim) * skillMultiplierAim;

            currentSpeedStrain *= decaySpeed;
            currentSpeedStrain += speedDifficulty * (1 - decaySpeed) * skillMultiplierSpeed;

            double totalStrain = DifficultyCalculationUtils.Norm(meanExponent, currentAimStrain, currentSpeedStrain);

            if (current.BaseObject is Slider)
                sliderStrains.Add(totalStrain);

            return totalStrain * skillMultiplierTotal;
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
