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
        private const float absolute_player_positioning_error = 12f;
        private const float normalized_hitobject_radius = 41.0f;

        protected override double SkillMultiplier => 490;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected override int SectionLength => 750;

        protected readonly float HalfCatcherWidth;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private double lastStrainTime;
        private bool previousIsDoubleHdash;
        public int lastMovementDirection;
        private bool isTapDashBoosted = false;
        private int tapDashDirection;
        private double tapDashStrainTimeAddition;
        private double tapDashDistanceAddition;
        private double lastExactDistanceMoved;
        private bool previousLastObjectWasHyperDash;
        private bool isBuzzSliderTriggered = false;

        public int DirectionChangeCount;

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

            lastPlayerPosition ??= catchCurrent.LastNormalizedPosition;

            float playerPosition = Math.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            bool isDoubleHdash = catchCurrent.LastObject.HyperDash && catchCurrent.BaseObject.HyperDash;

            float distanceMoved = playerPosition - lastPlayerPosition.Value;
            double exactDistanceMoved = catchCurrent.NormalizedPosition - lastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime / Math.Pow(catcherSpeedMultiplier, 0.45);
            double edgeDashBonus = 0;
            bool isSameDirection = Math.Sign(exactDistanceMoved) == Math.Sign(lastExactDistanceMoved);

            double movementValue;

            // Counting direction changes for length scaling in the performance calculation
            if (Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved) && Math.Sign(lastDistanceMoved) != 0 && Math.Abs(distanceMoved) > 0.1)
            {
                DirectionChangeCount += 1;
            }

            // Separating HyperDashes and basic fruits calculations
            // Basic fruit calculation
            if (!catchCurrent.LastObject.HyperDash)
            {
                // The base value is a ratio between distance moved and strain time
                movementValue = 0.065 * Math.Abs(distanceMoved) / weightedStrainTime;

                if (Math.Abs(distanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved) && Math.Sign(lastDistanceMoved) != 0)
                {
                    // We buff shorter movements upon direction change
                    movementValue *= 1 + (25 / Math.Pow(Math.Abs(exactDistanceMoved), 0.7));
                }
            }
            else
            {
                // Hyperdashes calculation
                // Both strain time and distance moved are scaled down because both factors are not optimally representing the difficulty
                movementValue = 0.078 * Math.Pow(Math.Abs(distanceMoved) / weightedStrainTime, 0.2);

                // Direction change scaling
                if (!previousLastObjectWasHyperDash && lastStrainTime > 0)
                    movementValue *= 1 + (0.3 * Math.Pow(Math.Abs(lastDistanceMoved), 1.15) / lastStrainTime);
                else movementValue *= 0.7;

                if (catchCurrent.LastObject.HyperDash && catchCurrent.BaseObject.HyperDash || isSameDirection)
                    movementValue *= 0.7;

                // Handling hyperdash chains
                if (previousLastObjectWasHyperDash)
               {
                    // Scaling hyperdash chains according to movement
                    movementValue *= isSameDirection ? Math.Pow(Math.Abs(distanceMoved), 0.5) / 6.5 : 10 / Math.Pow(Math.Abs(distanceMoved), 0.3);
               }
            }

            // We handle the case of tap-dashes ending with hdashes (stacks that require tapping in the same direction)
            if (catchCurrent.LastObject.HyperDash && Math.Abs(distanceMoved) > HalfCatcherWidth && isTapDashBoosted)
            {
                if (tapDashDirection == Math.Sign(distanceMoved))
                {
                    // Scaling factor is used to calculate the value using time spent since last "tap"
                    double tapDashScalingFactor = Math.Log(tapDashStrainTimeAddition -33, 1.3) - 0.07 * tapDashStrainTimeAddition - 7.8;
                    // Bonus is nerfed when the stack is not straight
                    double tapDashBonus = tapDashScalingFactor *  Math.Max(0, -0.03 * Math.Abs(tapDashDistanceAddition) + 1);
                    movementValue *= 1 + Math.Max(0, tapDashBonus);
                    isTapDashBoosted = false;
                }
                else isTapDashBoosted = false;
            }

            if (previousLastObjectWasHyperDash && Math.Abs(lastDistanceMoved) > 0 && distanceMoved == 0)
            {
                tapDashDirection = Math.Sign(lastDistanceMoved);
                tapDashStrainTimeAddition = 0;
                tapDashDistanceAddition = 0;
                isTapDashBoosted = true;
            }

            if (isTapDashBoosted)
            {
                tapDashStrainTimeAddition += weightedStrainTime;
                tapDashDistanceAddition += exactDistanceMoved;
            }

            // Edge dashes buff
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 8.2;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                // The edge dash bonus is scaled according to the distance to hyperdashes
                movementValue *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 45) * Math.Pow(Math.Min(catchCurrent.StrainTime, 265) / 265, 2) / Math.Max(1.0, Math.Pow(catcherSpeedMultiplier, 0.6)); // Edge Dashes are easier at lower ms values
            }

            if (Math.Abs(distanceMoved) <= absolute_player_positioning_error && Math.Abs(exactDistanceMoved) == Math.Abs(lastExactDistanceMoved) && catchCurrent.StrainTime == lastStrainTime)
            {
                if (isBuzzSliderTriggered) movementValue = 0;
                else isBuzzSliderTriggered = true;
            }
            else if (isBuzzSliderTriggered) isBuzzSliderTriggered = false;

            movementValue = Math.Pow(movementValue, 1.3);
            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;
            lastStrainTime = catchCurrent.StrainTime;
            previousIsDoubleHdash = isDoubleHdash;
            lastExactDistanceMoved = exactDistanceMoved;
            previousLastObjectWasHyperDash = catchCurrent.LastObject.HyperDash;

            return movementValue;
        }
    }
}
