// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            double applyDiminishingExp(double val) => Math.Pow(val, 0.99);

            if (Previous.Count > 0)
            {
                if (current.Angle != null && current.Angle.Value > angle_bonus_begin)
                {
                    var angleBonus = Math.Sqrt(
                        Math.Max(Previous[0].JumpDistance - scale, 0)
                        * Math.Pow(Math.Sin(current.Angle.Value - angle_bonus_begin), 2)
                        * Math.Max(current.JumpDistance - scale, 0));
                    result = 1.5 * applyDiminishingExp(Math.Max(0, angleBonus)) / Math.Max(timing_threshold, Previous[0].StrainTime);
                }
            }

            double jumpDistanceExp = applyDiminishingExp(current.JumpDistance);
            double travelDistanceExp = applyDiminishingExp(current.TravelDistance);

            return Math.Max(
                result + (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(current.StrainTime, timing_threshold),
                (Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / current.StrainTime
            );
        }
    }
}
