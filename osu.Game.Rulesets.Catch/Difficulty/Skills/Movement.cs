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
        private const double direction_change_bonus = 21.0;

        protected override double SkillMultiplier => 900;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private double lastStrainTime;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;
            double halfCatcherWidth = catchCurrent.HalfCatcherWidth;

            if (lastPlayerPosition == null)
                lastPlayerPosition = catchCurrent.LastNormalizedPosition;

            float playerPosition = MathHelper.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime + 18;

            double distanceAddition = (Math.Pow(Math.Abs(distanceMoved), 1.3) / 510);
            double sqrtStrain = Math.Sqrt(weightedStrainTime);

            double bonus = 0;

            // Direction changes give an extra point!
            if (Math.Abs(distanceMoved) > 0.1)
            {
                if (Math.Abs(lastDistanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    double bonusFactor = Math.Min(halfCatcherWidth, Math.Abs(distanceMoved)) / halfCatcherWidth;
                    double antiflowFactor = Math.Max(Math.Min(halfCatcherWidth, Math.Abs(lastDistanceMoved)) / halfCatcherWidth, 0.3);

                    distanceAddition += direction_change_bonus / Math.Sqrt(lastStrainTime + 18) * bonusFactor * antiflowFactor * Math.Max(1 - Math.Pow(weightedStrainTime / 1000, 2), 0);
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 12.5 * Math.Min(Math.Abs(distanceMoved), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Bonus for "almost" hyperdashes at corner points
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f / CatchPlayfield.BASE_WIDTH)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    bonus += 5.7;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + bonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash * CatchPlayfield.BASE_WIDTH) / 20) * Math.Pow((Math.Min(catchCurrent.StrainTime, 265) / 265), 1.5);
            }

            // Prevent wide dense stacks of notes which fit on the catcher from greatly increasing SR
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
