// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : Skill
    {
        private const float absolute_player_positioning_error = 16f;
        private const float normalized_hitobject_radius = 41.0f;

        protected override double SkillMultiplier => 900;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected readonly float HalfCatcherWidth;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private double lastStrainTime;

        public double DirectionChangeCount;

        //private double totalDistanceTraveled;

        public Movement(float halfCatcherWidth)
        {
            HalfCatcherWidth = halfCatcherWidth;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;

            lastPlayerPosition ??= catchCurrent.LastNormalizedPosition;

            float playerPosition = Math.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime + 7 + (1.5 / catchCurrent.ClockRate);

            // We do the base scaling according to the distance moved
            double distanceAddition = (Math.Pow(Math.Abs(distanceMoved), 0.87) / 210);

            double edgeDashBonus = 0;

            // Bonus for edge dashes.
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f)
            {
                // Bonus increased
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 6.5;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20)
                                                        * Math.Pow((Math.Min(catchCurrent.StrainTime * catchCurrent.ClockRate, 265) / 265), 1.5); // Edge Dashes are easier at lower ms values
            }

            double distanceRatioBonus;

            // Gives weight to non-hyperdashes
            if (!catchCurrent.LastObject.HyperDash)
            {
                // Speed is the ratio between "1/strain time" and the distance moved
                // So the larger and shorter a movement will be, the more it will be valued

                //Give value to long and fast movements

                distanceRatioBonus = 1.87 * Math.Abs(distanceMoved) / weightedStrainTime;

                double antiflowFactor = Math.Max(Math.Min(70, Math.Abs(lastDistanceMoved)) / 70, 0.38);

                if (Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    DirectionChangeCount += 1;

                    distanceRatioBonus *= antiflowFactor * 2.4;

                    // Give value to short movements if direction change
                    if (distanceMoved > 0.1 && distanceMoved < 50)
                    {
                        distanceRatioBonus += Math.Log(50 / Math.Abs(distanceMoved), 1.22) * antiflowFactor * 4.7;
                    }
                }
            }
            else // Hyperdashes calculation
            {
                distanceRatioBonus = Math.Abs(distanceMoved) / (2.4 * weightedStrainTime);
            }

            double distanceRatioBonusFactor = 4.95;

            // Hyperdashes - non hyperdashes variation bonus (custom bonus)
            if ((catchCurrent.BaseObject.HyperDash && !catchCurrent.LastObject.HyperDash) || (!catchCurrent.LastObject.HyperDash && catchCurrent.BaseObject.HyperDash))
                distanceRatioBonusFactor *= 1.2;

            distanceAddition *= distanceRatioBonusFactor * distanceRatioBonus;

            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;
            lastStrainTime = catchCurrent.StrainTime;

            return distanceAddition / weightedStrainTime;
        }
    }
}
