// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : Skill
    {
        private const float absolute_player_positioning_error = 16f;
        private const float normalized_hitobject_radius = 41.0f;
        private const double direction_change_bonus = 12.5;

        protected override double SkillMultiplier => 850;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected readonly float HalfCatcherWidth;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;

        public Movement(float halfCatcherWidth)
        {
            HalfCatcherWidth = halfCatcherWidth;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;

            if (lastPlayerPosition == null)
                lastPlayerPosition = catchCurrent.LastNormalizedPosition;

            float playerPosition = Math.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            double distanceAddition = Math.Pow(Math.Abs(distanceMoved), 1.3) / 500;
            double sqrtStrain = Math.Sqrt(catchCurrent.StrainTime);

            double bonus = 0;

            // Direction changes give an extra point!
            if (Math.Abs(distanceMoved) > 0.1)
            {
                if (Math.Abs(lastDistanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    double bonusFactor = Math.Min(absolute_player_positioning_error, Math.Abs(distanceMoved)) / absolute_player_positioning_error;

                    distanceAddition += direction_change_bonus / sqrtStrain * bonusFactor;

                    // Bonus for tougher direction switches and "almost" hyperdashes at this point
                    if (catchCurrent.LastObject.DistanceToHyperDash <= 10 / CatchPlayfield.BASE_WIDTH)
                        bonus = 0.3 * bonusFactor;
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 7.5 * Math.Min(Math.Abs(distanceMoved), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Bonus for "almost" hyperdashes at corner points
            if (catchCurrent.LastObject.DistanceToHyperDash <= 10.0f / CatchPlayfield.BASE_WIDTH)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    bonus += 1.0;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + bonus * ((10 - catchCurrent.LastObject.DistanceToHyperDash * CatchPlayfield.BASE_WIDTH) / 10);
            }

            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;

            return distanceAddition / catchCurrent.StrainTime;
        }
    }
}
