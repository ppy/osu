// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Aggregation;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public abstract class Aim : OsuProbabilitySkill
    {
        public readonly bool IncludeSliders;

        protected virtual double SkillMultiplier => 25.83;

        public Aim(Mod[] mods, bool includeSliders)
            : base(mods)
        {
            IncludeSliders = includeSliders;
        }

        private double currentStrain;
        protected override double FcProbability => 0.0005;
        private double strainDecayBase => 0.15;

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double HitProbability(double skill, double difficulty)
        {
            if (difficulty <= 0) return 1;
            if (skill <= 0) return 0;
            double result = DifficultyCalculationUtils.Erf(skill / (Math.Sqrt(2) * difficulty * 6));

            if (UseDifficultyPower) result = Math.Pow(result, 1 - 0.5 * DifficultyCalculationUtils.ReverseLerp(skill, 2500, 1000));

            return result * (UseDefaultMissProb ? (1 - 0.008 * DifficultyCalculationUtils.ReverseLerp(skill, 2500, 1000)) : 1.0);
        }

        private readonly List<double> sliderStrains = new List<double>();

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);

            if (current.BaseObject is Slider)
            {
                sliderStrains.Add(currentStrain);
            }

            currentStrain += StrainValueOf(current) * SkillMultiplier;

            if (double.IsNaN(currentStrain))
            {
                Console.WriteLine($"CLOWN: {current.BaseObject.StartTime}");
                double clown = StrainValueOf(current);
            }

            return currentStrain;
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
        protected abstract double StrainValueOf(DifficultyHitObject current);
    }
}
