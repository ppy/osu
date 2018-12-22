// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : Skill
    {
        private const double angle_bonus_begin = 3 * Math.PI / 4;
        private const double pi_over_4 = Math.PI / 4;

        protected override double SkillMultiplier => 1400;
        protected override double StrainDecayBase => 0.3;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45; // ~330BPM
        private const double speed_balancing_factor = 40;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double distance = Math.Min(SINGLE_SPACING_THRESHOLD, current.TravelDistance + current.JumpDistance);
            double deltaTime = Math.Max(max_speed_bonus, current.DeltaTime);

            double speedBonus = 1.0;
            if (deltaTime < min_speed_bonus)
                speedBonus = 1 + Math.Pow((min_speed_bonus - deltaTime) / speed_balancing_factor, 2);

            double angleBonus = 1.0;
            if (current.Angle != null && current.Angle.Value < angle_bonus_begin)
            {
                angleBonus = 1 + Math.Min(Math.Sin(angle_bonus_begin - current.Angle.Value), Math.Sin(Math.PI / 4)) / 2.5;
                if (distance < 90 && current.Angle.Value < Math.PI / 4)
                    angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1);
                else if (distance < 90 && current.Angle.Value < Math.PI / 2)
                    angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1) * Math.Sin((Math.PI / 2 - current.Angle.Value) / pi_over_4);
            }

            return speedBonus * angleBonus * (0.95 + Math.Pow(distance / SINGLE_SPACING_THRESHOLD, 4)) / current.StrainTime;
        }
    }
}
