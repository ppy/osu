// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Reading : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.4;

        private double currentStrain;

        public double ObjectDensity;

        private const double high_sv_multiplier = 1.0;

        /// <summary>
        /// Creates a <see cref="Rhythm"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        public Reading(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            currentStrain *= StrainDecayBase;
            currentStrain += reading(current) * SkillMultiplier;

            return currentStrain;
        }

        private double reading(DifficultyHitObject current)
        {
            TaikoDifficultyHitObject noteObject = (TaikoDifficultyHitObject)current;

            if (noteObject.BaseObject is Swell || noteObject.BaseObject is DrumRoll)
            {
                return 1;
            }

            // Only return a difficulty value when the Object isn't a Spinner or a Slider.
            double sliderVelocityBonus = calculateHighVelocityBonus(noteObject.EffectiveBPM);
            ObjectDensity = calculateObjectDensity(noteObject.DeltaTime, noteObject.EffectiveBPM, noteObject.CurrentSliderVelocity);

            return high_sv_multiplier * sliderVelocityBonus;
        }

        /// <summary>
        /// Calculates the influence of higher slider velocities on beatmap difficulty.
        /// The bonus is determined based on the EffectiveBPM, shifting within a defined range
        /// between the upper and lower boundaries to reflect how increased slider velocity impacts difficulty.
        /// </summary>
        private double calculateHighVelocityBonus(double effectiveBPM)
        {
            // The maximum and minimum center value for the impact of EffectiveBPM.
            const double velocity_max = 640;
            const double velocity_min = 480;

            const double center = (velocity_max + velocity_min) / 2;
            const double range = velocity_max - velocity_min;

            return sigmoid(effectiveBPM, center, range);
        }

        private double calculateObjectDensity(double deltaTime, double effectiveBPM, double currentSliderVelocity)
        {
            // The maximum and minimum center value for density.
            const double density_max = 300;
            const double density_min = 50;

            const double center = 200;
            const double range = 2000;

            // Adjusts the penalty for low SV based on object density.
            return density_max - (density_max - density_min) *
                sigmoid(deltaTime * currentSliderVelocity, center, range);
        }

        /// <summary>
        /// Calculates a smooth transition using a sigmoid function.
        /// <param name="value">The input value</param>
        /// <param name="center">The midpoint of the curve where the output transitions most rapidly.</param>
        /// <param name="range">Determines how steep or gradual the curve is around the center.</param>
        /// </summary>
        private double sigmoid(double value, double center, double range)
        {
            range /= 10;
            return 1 / (1 + Math.Exp(-(value - center) / range));
        }
    }
}
