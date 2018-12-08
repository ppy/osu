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

            if (current.Angle != null)
            {
                double angle = current.Angle.Value * 180 / Math.PI;

                if (angle < 135)
                    angleBonus = (135 - angle / 45) * 0.25 + 1.0;
                else if (angle <= 90)
                    angleBonus = 1.25;
            }

            return angleBonus * (0.95 + Math.Pow(distance / SINGLE_SPACING_THRESHOLD, 4)) / current.StrainTime;
        }
    }
}
