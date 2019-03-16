// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osuTK;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : Skill
    {
        private const float absolute_player_positioning_error = 16f;
        private const float normalized_hitobject_radius = 41.0f;
        private const double direction_change_bonus = 9.5;
        private const double antiflow_bonus = 25.0;

        protected override double SkillMultiplier => 850;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private double lastStrainTime;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;

            if (lastPlayerPosition == null)
                lastPlayerPosition = catchCurrent.LastNormalizedPosition;

            float playerPosition = MathHelper.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            // Reduce speed scaling
            double weightedStrainTime = catchCurrent.StrainTime + 20;

            double distanceAddition = Math.Pow(Math.Abs(distanceMoved), 1.3) / 600;
            double sqrtStrain = Math.Sqrt(weightedStrainTime);

            double bonus = 0;

            // Direction changes give an extra point!
            if (Math.Abs(distanceMoved) > 0.1)
            {
                if (Math.Abs(lastDistanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    double bonusFactor = Math.Min(absolute_player_positioning_error, Math.Abs(distanceMoved)) / absolute_player_positioning_error;

                    distanceAddition += direction_change_bonus / sqrtStrain * bonusFactor;

                    // Direction changes after jumps (antiflow) are harder
                    double antiflowBonusFactor = Math.Min(Math.Abs(distanceMoved) / 70, 1);

                    distanceAddition += (antiflow_bonus / (catchCurrent.StrainTime / 40 + 10)) * (Math.Sqrt(Math.Abs(lastDistanceMoved)) / Math.Sqrt(lastStrainTime + 20)) * antiflowBonusFactor;
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 10.0 * Math.Min(Math.Abs(distanceMoved), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Big bonus for edge hyperdashes
            if (catchCurrent.LastObject.DistanceToHyperDash <= 14.0f / CatchPlayfield.BASE_WIDTH)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    bonus += 5.0;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + bonus * (14 - catchCurrent.LastObject.DistanceToHyperDash * CatchPlayfield.BASE_WIDTH) / 14 * (Math.Min(catchCurrent.StrainTime, 180) / 180); // Edge dashes are easier at lower ms values
            }

            // Prevent wide, dense stacks of notes which fit on the catcher from greatly increasing SR
            if (Math.Abs(distanceMoved) > 0.1)
            {
                if (Math.Abs(lastDistanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    if (Math.Abs(distanceMoved) <= (CatcherArea.CATCHER_SIZE) && Math.Abs(lastDistanceMoved) == Math.Abs(distanceMoved))
                    {
                        if (catchCurrent.StrainTime <= 80 && lastStrainTime == catchCurrent.StrainTime)
                        {
                            distanceAddition *= Math.Max(((catchCurrent.StrainTime / 80) - 0.75) * 4, 0);
                        }
                    }
                }
            }

            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;
            lastStrainTime = catchCurrent.StrainTime;

            return distanceAddition / weightedStrainTime;
        }
    }
}
