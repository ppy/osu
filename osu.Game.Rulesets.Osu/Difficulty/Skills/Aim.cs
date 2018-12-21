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
        private const double angle_bonus_begin = 5 * Math.PI / 12;

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double angleBonus = 0;

            double result = 0;

            if (Previous.Count > 0)
            {
                if (current.Angle != null)
                    angleBonus = (Previous[0].JumpDistance - STREAM_SPACING_THRESHOLD) * Math.Sin(current.Angle.Value - angle_bonus_begin);
                result = Math.Pow(Math.Max(0, angleBonus), 0.99) / Previous[0].StrainTime;
            }

            return result + (Math.Pow(current.TravelDistance, 0.99) + Math.Pow(current.JumpDistance, 0.99)) / current.StrainTime;
        }
    }
}
