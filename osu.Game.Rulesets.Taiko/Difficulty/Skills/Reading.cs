// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Reading : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.4;

        private double currentStrain;

        public static double ObjectDensity;

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
            TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)current;

            // Calculate High SV and Low SV Object Density,
            double sliderVelocityBonus = calculateHighVelocityBonus(hitObject.EffectiveBPM);
            ObjectDensity = calculateObjectDensity(current.DeltaTime);

            return high_sv_multiplier * sliderVelocityBonus;
        }

        /// <summary>
        /// Calculates the influence of higher slider velocities on beatmap difficulty.
        /// The bonus is determined based on the EffectiveBPM, shifting within a defined range
        /// between the upper and lower boundaries to reflect how increased slider velocity impacts difficulty.
        /// </summary>
        private double calculateHighVelocityBonus(double effectiveBPM)
        {
            const double velocity_max = 640;
            const double velocity_min = 480;

            const double center = (velocity_max + velocity_min) / 2;
            const double range = velocity_max - velocity_min;

            return sigmoid(effectiveBPM, center, range);
        }

        private double calculateObjectDensity(double deltaTime)
        {
            // The maximum and minimum center value for density.
            const double density_max = 150;
            const double density_min = 50;

            // The midpoint of the range for the sigmoid function, where the transition is most significant.
            const double center = 200;
            const double range = 300;

            // Adjusts the penalty for low SV based on object density.
            return density_max - (density_max - density_min) *
                sigmoid(deltaTime, center, range);
        }

        /// <summary>
        /// Calculates a smooth transition using a sigmoid function.
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
