// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : Skill
    {
        private const double min_angle_bonus = 0;
        private const double max_angle_bonus = 0.5;
        private const double angle_bonus_begin = 5 * Math.PI / 12;
        private const double pi_over_2 = Math.PI / 2;

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double angleBonus = 0;

            if (current.Angle != null)
                angleBonus = MathHelper.Clamp((current.Angle.Value - angle_bonus_begin) / pi_over_2, min_angle_bonus, max_angle_bonus);

            return (angleBonus * Math.Pow(Math.Max(0, current.JumpDistance - STREAM_SPACING_THRESHOLD), 0.99)
                       + Math.Pow(current.TravelDistance, 0.99)
                       + Math.Pow(current.JumpDistance, 0.99)
                   ) / current.StrainTime;
        }
    }
}
