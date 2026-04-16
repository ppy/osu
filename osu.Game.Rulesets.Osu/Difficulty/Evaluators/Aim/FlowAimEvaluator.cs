// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class FlowAimEvaluator
    {
        private const double velocity_change_multiplier = 0.52;

        /// <summary>
        /// Evaluates difficulty of "flow aim" - aiming pattern where player doesn't stop their cursor on every object and instead "flows" through them.
        /// </summary>
        public static double EvaluateDifficultyOf(OsuDifficultyHitObject currObj, bool withSliderTravelDistance)
        {
            if (currObj.BaseObject is Spinner || currObj.Index <= 1 || currObj.Previous(0).BaseObject is Spinner)
                return 0;

            var nextObj = (OsuDifficultyHitObject?)currObj.Next(0);
            var prevObj = (OsuDifficultyHitObject)currObj.Previous(0);
            var prev2Obj = (OsuDifficultyHitObject)currObj.Previous(1);

            double currDistance = withSliderTravelDistance ? currObj.LazyJumpDistance : currObj.JumpDistance;
            double prevDistance = withSliderTravelDistance ? prevObj.LazyJumpDistance : prevObj.JumpDistance;

            double currVelocity = currDistance / currObj.AdjustedDeltaTime;

            if (prevObj.BaseObject is Slider && withSliderTravelDistance)
            {
                // If the last object is a slider, then we extend the travel velocity through the slider into the current object.
                double sliderDistance = prevObj.LazyTravelDistance + currObj.LazyJumpDistance;
                currVelocity = Math.Max(currVelocity, sliderDistance / currObj.AdjustedDeltaTime);
            }

            double prevVelocity = prevDistance / prevObj.AdjustedDeltaTime;

            double flowDifficulty = currVelocity;

            // Apply high circle size bonus to the base velocity.
            // We use reduced CS bonus here because the bonus was made for an evaluator with a different d/t scaling
            flowDifficulty *= Math.Sqrt(currObj.SmallCircleBonus);

            // Rhythm changes are harder to flow
            flowDifficulty *= 1 + Math.Min(0.25,
                Math.Pow((Math.Max(currObj.AdjustedDeltaTime, prevObj.AdjustedDeltaTime) - Math.Min(currObj.AdjustedDeltaTime, prevObj.AdjustedDeltaTime)) / 50, 4));

            if (currObj.Angle != null && prevObj.Angle != null)
            {
                double angleDifference = Math.Abs(currObj.Angle.Value - prevObj.Angle.Value);
                double angleDifferenceAdjusted = Math.Sin(angleDifference / 2) * 180.0;
                double angularVelocity = angleDifferenceAdjusted / (currObj.AdjustedDeltaTime * 0.1);

                // Low angular velocity flow (angles are consistent) is easier to follow than erratic flow
                flowDifficulty *= 0.8 + Math.Sqrt(angularVelocity / 270.0);
            }

            // If all three notes are overlapping - don't reward bonuses as you don't have to do additional movement
            double overlappedNotesWeight = 1;

            if (currObj.Index > 2)
            {
                double o1 = calculateOverlapFactor(currObj, prevObj);
                double o2 = calculateOverlapFactor(currObj, prev2Obj);
                double o3 = calculateOverlapFactor(prevObj, prev2Obj);

                overlappedNotesWeight = 1 - o1 * o2 * o3;
            }

            if (currObj.Angle != null)
            {
                // Acute angles are also hard to flow
                // We square root velocity to make acute angle switches in streams aren't having difficulty higher than snap
                flowDifficulty += Math.Sqrt(currVelocity) *
                                  SnapAimEvaluator.CalcAngleAcuteness(currObj.Angle.Value) *
                                  overlappedNotesWeight;
            }

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                if (withSliderTravelDistance)
                {
                    currVelocity = currDistance / currObj.AdjustedDeltaTime;
                }

                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = DifficultyCalculationUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25 / Math.Min(currObj.AdjustedDeltaTime, prevObj.AdjustedDeltaTime),
                    Math.Abs(prevVelocity - currVelocity));

                flowDifficulty += overlapVelocityBuff *
                                  distRatio *
                                  overlappedNotesWeight *
                                  velocity_change_multiplier;
            }

            if (currObj.BaseObject is Slider && withSliderTravelDistance)
            {
                // Include slider velocity to make velocity more consistent with snap
                flowDifficulty += currObj.TravelDistance / currObj.TravelTime;
            }

            // Final velocity is being raised to a power because flow difficulty scales harder with both high distance and time, and we want to account for that
            flowDifficulty = Math.Pow(flowDifficulty, 1.45);

            // Reduce difficulty for low spacing since spacing below radius is always to be flowed
            return flowDifficulty * DifficultyCalculationUtils.Smootherstep(currDistance, 0, OsuDifficultyHitObject.NORMALISED_RADIUS);
        }

        private static double calculateOverlapFactor(OsuDifficultyHitObject first, OsuDifficultyHitObject second)
        {
            var firstBase = (OsuHitObject)first.BaseObject;
            var secondBase = (OsuHitObject)second.BaseObject;
            double objectRadius = firstBase.Radius;

            double distance = Vector2.Distance(firstBase.StackedPosition, secondBase.StackedPosition);
            return Math.Clamp(1 - Math.Pow(Math.Max(distance - objectRadius, 0) / objectRadius, 2), 0, 1);
        }
    }
}
