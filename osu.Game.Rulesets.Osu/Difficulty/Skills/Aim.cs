// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : Skill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double result = 0;

            const double scale = 90;

            if (Previous.Count > 0)
            {
                if (current.Angle != null && current.Angle.Value > angle_bonus_begin)
                {
                    var angleBonus = Math.Sqrt(
                        Math.Max(Previous[0].JumpDistance - scale, 0)
                        * Math.Pow(Math.Sin(current.Angle.Value - angle_bonus_begin), 2)
                        * Math.Max(current.JumpDistance - scale, 0));
                    result = 1.5 * Math.Pow(Math.Max(0, angleBonus), 0.99) / Math.Max(timing_threshold, Previous[0].StrainTime);
                }
            }

            return Math.Max(
                result + (
                    Math.Pow(current.JumpDistance, 0.99)
                    + Math.Pow(current.TravelDistance, 0.99)
                    + Math.Sqrt(Math.Pow(current.TravelDistance, 0.99) * Math.Pow(current.JumpDistance, 0.99)))
                / Math.Max(current.StrainTime, timing_threshold),
                (Math.Sqrt(Math.Pow(current.TravelDistance, 0.99) * Math.Pow(current.JumpDistance, 0.99))
                 + Math.Pow(current.JumpDistance, 0.99)
                 + Math.Pow(current.TravelDistance, 0.99))
                / current.StrainTime);
        }
    }
}
