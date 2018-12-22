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
        private const double angle_bonus_begin = 5 * Math.PI / 12;
        private const double timing_threshold = 107;
        private const double min_distance_for_bonus = 90;
        private const double angle_threshold = Math.PI / 4;

        private static readonly double sin_angle_threshold = Math.Sin(angle_threshold);

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double result = 0;

            if (Previous.Count > 0)
            {
                if (current.Angle != null && current.Angle.Value > angle_bonus_begin)
                {
                    var sinDiffAngle = Math.Sin(current.Angle.Value - angle_bonus_begin);

                    var angleBonus = Math.Sqrt
                    (
                        Math.Max(0, Previous[0].JumpDistance + Previous[0].TravelDistance - min_distance_for_bonus)
                        * Math.Min
                        (
                            sinDiffAngle,
                            sin_angle_threshold
                        )
                        * Math.Max(0, current.JumpDistance - min_distance_for_bonus)
                        * Math.Min
                        (
                            sinDiffAngle,
                            sin_angle_threshold
                        )
                    );

                    result = 2 * Math.Pow(Math.Max(0, angleBonus), 0.99) / Math.Max(Previous[0].StrainTime, timing_threshold);
                }
            }

            return Math.Max
            (
                result + (Math.Pow(current.TravelDistance, 0.99) + Math.Pow(current.JumpDistance, 0.99)) / Math.Max(current.StrainTime, timing_threshold),
                (Math.Pow(current.TravelDistance, 0.99) + Math.Pow(current.JumpDistance, 0.99)) / current.StrainTime
            );
        }
    }
}
