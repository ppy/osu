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
        private const double direction_change_bonus = 9.8;
        private const double antiflow_bonus = 26.0;

        protected override double SkillMultiplier => 860;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private double lastStrainTime;
        private bool lastHyperdash;

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

            double distanceAddition = Math.Pow(Math.Abs(distanceMoved), 1.2) / 340;
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

                    distanceAddition += (antiflow_bonus / (catchCurrent.StrainTime / 17.5 + 10)) * (Math.Sqrt(Math.Abs(lastDistanceMoved)) / Math.Sqrt(lastStrainTime + 25)) * antiflowBonusFactor;

                    // Reduce strain slightly for Hyperdash chains
                    if (catchCurrent.LastObject.HyperDash && lastHyperdash)
                        distanceAddition *= 0.95;

                    if (catchCurrent.LastObject.DistanceToHyperDash <= 14.0f / CatchPlayfield.BASE_WIDTH && !catchCurrent.LastObject.HyperDash)
                        bonus += 3.0;
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 10.0 * Math.Min(Math.Abs(distanceMoved), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Big bonus for edge dashes
            if (catchCurrent.LastObject.DistanceToHyperDash <= 14.0f / CatchPlayfield.BASE_WIDTH)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    bonus += 4.5;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + bonus * (14.0f - catchCurrent.LastObject.DistanceToHyperDash * CatchPlayfield.BASE_WIDTH) / 14.0f * (Math.Min(catchCurrent.StrainTime, 250) / 250); // Edge dashes are easier at lower ms values
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
            lastHyperdash = catchCurrent.LastObject.HyperDash;

            return distanceAddition / weightedStrainTime;
        }
    }
}
