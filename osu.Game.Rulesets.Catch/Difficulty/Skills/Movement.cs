// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : StrainDecaySkill
    {
        private const double direction_change_bonus = 21.0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected override int SectionLength => 750;

        protected readonly float HalfCatcherWidth;

        private bool isInBuzzSection;

        /// <summary>
        /// The speed multiplier applied to the player's catcher.
        /// </summary>
        private readonly double catcherSpeedMultiplier;

        public Movement(Mod[] mods, float halfCatcherWidth, double clockRate)
            : base(mods)
        {
            HalfCatcherWidth = halfCatcherWidth;

            // In catch, clockrate adjustments do not only affect the timings of hitobjects,
            // but also the speed of the player's catcher, which has an impact on difficulty
            // TODO: Support variable clockrates caused by mods such as ModTimeRamp
            //  (perhaps by using IApplicableToRate within the CatchDifficultyHitObject constructor to set a catcher speed for each object before processing)
            catcherSpeedMultiplier = clockRate;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;
            var catchLast = (CatchDifficultyHitObject)current.Previous(0);

            double weightedStrainTime = catchCurrent.StrainTime + 13 + (3 / catcherSpeedMultiplier);

            double distanceAddition = (Math.Pow(Math.Abs(catchCurrent.DistanceMoved), 1.3) / 510);
            double sqrtStrain = Math.Sqrt(weightedStrainTime);

            double edgeDashBonus = 0;

            // Direction change bonus.
            if (Math.Abs(catchCurrent.DistanceMoved) > 0.1)
            {
                if (current.Index >= 1 && Math.Abs(catchLast.DistanceMoved) > 0.1 && Math.Sign(catchCurrent.DistanceMoved) != Math.Sign(catchLast.DistanceMoved))
                {
                    double bonusFactor = Math.Min(50, Math.Abs(catchCurrent.DistanceMoved)) / 50;
                    double antiflowFactor = Math.Max(Math.Min(70, Math.Abs(catchLast.DistanceMoved)) / 70, 0.38);

                    distanceAddition += direction_change_bonus / Math.Sqrt(catchLast.StrainTime + 16) * bonusFactor * antiflowFactor * Math.Max(1 - Math.Pow(weightedStrainTime / 1000, 3), 0);
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 12.5 * Math.Min(Math.Abs(catchCurrent.DistanceMoved), CatchDifficultyHitObject.NORMALIZED_HALF_CATCHER_WIDTH * 2)
                                    / (CatchDifficultyHitObject.NORMALIZED_HALF_CATCHER_WIDTH * 6) / sqrtStrain;
            }

            // Bonus for edge dashes.
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 5.7;

                distanceAddition *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20)
                                                        * Math.Pow((Math.Min(catchCurrent.StrainTime * catcherSpeedMultiplier, 265) / 265), 1.5); // Edge Dashes are easier at lower ms values
            }

            // There is an edge case where horizontal back and forth sliders create "buzz" patterns which are repeated "movements" with a distance lower than
            // the platter's width but high enough to be considered a movement due to the absolute_player_positioning_error and NORMALIZED_HALF_CATCHER_WIDTH offsets
            // We are detecting this exact scenario. The first back and forth is counted but all subsequent ones are nullified.
            // To achieve that, we need to store the exact distances (distance ignoring absolute_player_positioning_error and NORMALIZED_HALF_CATCHER_WIDTH)
            if (current.Index >= 1 && Math.Abs(catchCurrent.ExactDistanceMoved) <= CatchDifficultyHitObject.NORMALIZED_HALF_CATCHER_WIDTH * 2
                                   && catchCurrent.ExactDistanceMoved == -catchLast.ExactDistanceMoved && catchCurrent.StrainTime == catchLast.StrainTime)
            {
                if (isInBuzzSection)
                    distanceAddition = 0;
                else
                    isInBuzzSection = true;
            }
            else
            {
                isInBuzzSection = false;
            }

            return distanceAddition / weightedStrainTime;
        }
    }
}
