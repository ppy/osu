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

            double applyDiminishingExp(double val) => Math.Pow(val, 0.99);

            if (Previous.Count > 0)
            {
                    
                    current.Angle.Value = current.Angle.Value * Math.Max(0,Math.Min(1,(-1 * Previous[0].StrainTime / 50 + 4)))
                    
                    var angleBonus = ((2 / (1 + Math.Pow(3, -1 * current.Angle.Value)))-0.25)
                    * Math.Min(Math.Max(Previous[0].JumpDistance / 30 - 1, 0),1)
                    * Math.Min(Math.Max(current.JumpDistance / 30 - 1, 0),1);
                    
                    result = angleBonus;
                
            }

            double jumpDistanceExp = applyDiminishingExp(current.JumpDistance);
            double travelDistanceExp = applyDiminishingExp(current.TravelDistance);

            return Math.Max(
                result * (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(current.StrainTime, timing_threshold),
                (Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / current.StrainTime
            );
        }
    }
}

