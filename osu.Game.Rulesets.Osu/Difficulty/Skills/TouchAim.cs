// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class TouchAim : Skill
    {
        private const double timing_threshold = 107;

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            double jump = osuCurrent.JumpDistance;
            double angle = 0.0;

            if (Previous.Count > 1)
            {
                var osuPrevious = (OsuDifficultyHitObject)Previous[0];

                if (osuCurrent.Angle != null)
                {
                    // Angle bonus is reduced if previous two objects are too slow
                    angle = osuCurrent.Angle.Value * (Math.Max(0, Math.Min(1, (-1 * osuPrevious.StrainTime / 75 + 4))));
                    // Angle bonus is reduced if previous two objects are too close
                    angle *= (Math.Min(Math.Max((2 * osuPrevious.JumpDistance) / Math.Max(1, osuCurrent.JumpDistance), 0), 1));
                    // Bonus is added for notes that require tapX
                    angle += Math.PI * (1 / (1 + Math.Pow(1.06, (2.5 * Math.Min(osuPrevious.StrainTime, 90) - 120))) 
                             * (Math.Min(1, Math.Max(0, -0.015 * osuPrevious.JumpDistance + 1.6))));
                }

                angle *= 180 / Math.PI; // Converting radians to degrees

                // Angle bonus is applied by scaling jump distance for aim
                if (angle > 120)
                    jump *= 1.9 + 0.05 * (angle - 120) / 60;
                else if (angle > 90)
                    jump *= 1.7 + 0.2 * (angle - 90) / 30;
                else if (angle > 15)
                    jump *= 0.95 + 0.75 * (angle - 15) / 75;
                else
                    jump *= 0.45 + 0.5 * angle / 15;

            }

            double jumpDistanceExp = applyDiminishingExp(jump);
            double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance);

            return Math.Max(
                (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(osuCurrent.StrainTime, timing_threshold),
                (Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / osuCurrent.StrainTime
            );
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
