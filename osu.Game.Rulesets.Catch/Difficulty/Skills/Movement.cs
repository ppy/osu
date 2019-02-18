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

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;
            var catchPrevious = (CatchDifficultyHitObject)Previous[0];

            var sqrtStrain = Math.Sqrt(catchCurrent.StrainTime);

            double distanceAddition = Math.Pow(Math.Abs(catchCurrent.MovementDistance), 1.3) / 500;

            double bonus = 0;

            if (Math.Abs(catchCurrent.MovementDistance) > 0.1)
            {
                if (catchPrevious.MovementDistance > 0.1 && Math.Sign(catchCurrent.MovementDistance) != Math.Sign(catchPrevious.MovementDistance))
                {
                    double bonusFactor = Math.Min(absolute_player_positioning_error, Math.Abs(catchCurrent.MovementDistance)) / absolute_player_positioning_error;

                    distanceAddition += direction_change_bonus / sqrtStrain * bonusFactor;

                    // Bonus for tougher direction switches and "almost" hyperdashes at this point
                    if (catchPrevious.BaseObject.DistanceToHyperDash <= 10 / CatchPlayfield.BASE_WIDTH)
                        bonus = 0.3 * bonusFactor;
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 7.5 * Math.Min(Math.Abs(catchCurrent.MovementDistance), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Bonus for "almost" hyperdashes at corner points
            if (catchPrevious.BaseObject.DistanceToHyperDash <= 10.0f / CatchPlayfield.BASE_WIDTH)
            {
                if (!catchPrevious.BaseObject.HyperDash)
                    bonus += 1.0;

                distanceAddition *= 1.0 + bonus * ((10 - catchPrevious.BaseObject.DistanceToHyperDash * CatchPlayfield.BASE_WIDTH) / 10);
            }

            return distanceAddition / catchCurrent.StrainTime;
        }
    }
}
