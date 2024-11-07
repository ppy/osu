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

        public static double LowSvBonus;
        public static double HighSvBonus;
        public static double ObjectDensity;

        private double currentStrain;

        private const double high_sv_multiplier = 1.0;
        private const double low_sv_multiplier = 1.0;

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

            // Variables for high SV influence.
            const double high_sv_upper = 640; // The upper BPM threshold where high SV influence starts to fade.
            const double high_sv_lower = 480; // The lower BPM threshold where high SV influence becomes significant.

            // Center and range for the high SV sigmoid curve, making the impact gradual within this range.
            const double high_sv_center = (high_sv_upper + high_sv_lower) / 2;
            const double high_sv_range = high_sv_upper - high_sv_lower;

            // Variables for low SV influence.
            const double low_sv_delta_time_center = 200; // The delta time around which low SV influence centers.
            const double low_sv_delta_time_range = 300; // The range for the transition around the delta time.

            // Center range for low SV adjustment based on hit object density.
            const double low_sv_max_center = 150; // Upper bound for low SV adjustment.
            const double low_sv_min_center = 50; // Lower bound for low SV adjustment.
            const double low_sv_range = 240; // Reduced range to make the impact more abrupt at very low SVs.

            // Calculate the adjusted center for low SV influence based on object density.
            ObjectDensity = low_sv_max_center - (low_sv_max_center - low_sv_min_center)
                * sigmoid(current.DeltaTime, low_sv_delta_time_center, low_sv_delta_time_range);

            // Calculate bonuses based on high and low SV impact.
            HighSvBonus = sigmoid(hitObject.EffectiveBPM, high_sv_center, high_sv_range);
            LowSvBonus = 1 - sigmoid(hitObject.EffectiveBPM, ObjectDensity, low_sv_range);

            return high_sv_multiplier * HighSvBonus; // No bonuses to lowSV within starRating.
        }

        private double sigmoid(double value, double center, double range)
        {
            range /= 10;
            return 1 / (1 + Math.Exp(-(value - center) / range));
        }
    }
}
