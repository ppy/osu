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

        private const double high_sv_multiplier = 1;
        private const double low_sv_multiplier = 1;

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
            currentStrain += readingBonus(current) * SkillMultiplier;

            return currentStrain;
        }

        private double readingBonus(DifficultyHitObject current)
        {
            TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)current;

            // High SV Variables
            const double high_sv_upper_bound = 640;
            const double high_sv_lower_bound = 480;

            const double high_sv_center = (high_sv_upper_bound + high_sv_lower_bound) / 2;
            const double high_sv_width = high_sv_upper_bound - high_sv_lower_bound;

            // Low SV Variables
            const double low_sv_delta_time_center = 200;
            const double low_sv_delta_time_width = 300;

            // Maximum center for low sv (for high density)
            const double low_sv_center_upper_bound = 200;

            // Minimum center for low sv (for low density)
            const double low_sv_center_lower_bound = 100;
            const double low_sv_width = 160;

            // Calculate low sv center, considering density
            double lowSvCenter = low_sv_center_upper_bound - (low_sv_center_upper_bound - low_sv_center_lower_bound) * this.sigmoid(current.DeltaTime, low_sv_delta_time_center, low_sv_delta_time_width);
            double highSvBonus = sigmoid(hitObject.EffectiveBPM, high_sv_center, high_sv_width);
            double lowSvBonus = 1 - sigmoid(hitObject.EffectiveBPM, lowSvCenter, low_sv_width);

            return high_sv_multiplier * highSvBonus + low_sv_multiplier * lowSvBonus;
        }

        private double sigmoid(double value, double center, double width)
        {
            width /= 10;
            return 1 / (1 + Math.Exp(-(value - center) / width));
        }
    }
}
