// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class FlowAimEvaluator
    {
        /// <summary>
        /// Evaluates difficulty of "flow aim" - aiming pattern where player doesn't stop their cursor on every object and instead "flows" through them.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            double currDistance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double prevDistance = withSliderTravelDistance ? osuLastObj.LazyJumpDistance : osuLastObj.JumpDistance;

            double currVelocity = calculateCurrentVelocity(osuCurrObj, osuLastObj, currDistance, withSliderTravelDistance);
            double prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;

            double flowDifficulty = currVelocity;

            // Apply high circle size bonus to the base velocity.
            // We use reduced CS bonus here because the bonus was made for an evaluator with a different d/t scaling
            flowDifficulty *= Math.Sqrt(osuCurrObj.SmallCircleBonus);

            flowDifficulty *= calculateRhythmChangeBonus(osuCurrObj, osuLastObj);
            flowDifficulty *= calculateAngularVelocityBonus(osuCurrObj, osuLastObj);

            // If all three notes are overlapping - don't reward bonuses as you don't have to do additional movement
            double overlappedNotesWeight = calculateOverlappedNotesWeight(osuCurrObj, osuLastObj);

            flowDifficulty += calculateAcuteAngleBonus(osuCurrObj, currVelocity, overlappedNotesWeight);
            flowDifficulty += calculateVelocityChangeBonus(osuCurrObj, osuLastObj, currVelocity, prevVelocity, currDistance, overlappedNotesWeight, withSliderTravelDistance);
            flowDifficulty += calculateSliderBonus(osuCurrObj, withSliderTravelDistance);

            // Final velocity is being raised to a power because flow difficulty scales harder with both high distance and time, and we want to account for that
            flowDifficulty = DiffUtils.Pow(flowDifficulty, 1.45);

            // Reduce difficulty for low spacing since spacing below radius is always to be flowed
            return flowDifficulty * DiffUtils.Smootherstep(currDistance, 0, OsuDifficultyHitObject.NORMALISED_RADIUS);
        }

        private static double calculateRhythmChangeBonus(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj) =>
            1 + Math.Min(0.25,
                DiffUtils.Pow((Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) - Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime)) / 50, 4));

        /// <summary>
        /// Scales flow difficulty by angular velocity.
        /// This nerfs consistent angles whilst buffing "erratic" flow.
        /// </summary>
        private static double calculateAngularVelocityBonus(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj)
        {
            if (osuCurrObj.Angle == null || osuLastObj.Angle == null)
                return 1;

            double angleDifference = Math.Abs(osuCurrObj.Angle.Value - osuLastObj.Angle.Value);
            double angleDifferenceAdjusted = Math.Sin(angleDifference / 2) * 180.0;
            double angularVelocity = angleDifferenceAdjusted / (osuCurrObj.AdjustedDeltaTime * 0.1);

            return 0.8 + Math.Sqrt(angularVelocity / 270.0);
        }

        private static double calculateAcuteAngleBonus(OsuDifficultyHitObject osuCurrObj, double currVelocity, double overlappedNotesWeight)
        {
            if (osuCurrObj.Angle == null)
                return 0;

            return currVelocity * SnapAimEvaluator.CalcAngleAcuteness(osuCurrObj.Angle.Value) * overlappedNotesWeight;
        }

        private static double calculateVelocityChangeBonus(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj, double currVelocity, double prevVelocity,
                                                           double currDistance, double overlappedNotesWeight, bool withSliderTravelDistance)
        {
            const double velocity_change_multiplier = 0.52;

            if (Math.Max(prevVelocity, currVelocity) == 0)
                return 0;

            if (withSliderTravelDistance)
                currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;

            // Scale with ratio of difference compared to 0.5 * max dist.
            double distRatio = DiffUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

            // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
            double overlapVelocityBuff = Math.Min(OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime),
                Math.Abs(prevVelocity - currVelocity));

            return overlapVelocityBuff * distRatio * overlappedNotesWeight * velocity_change_multiplier;
        }

        private static double calculateSliderBonus(OsuDifficultyHitObject osuCurrObj, bool withSliderTravelDistance)
        {
            if (osuCurrObj.BaseObject is not Slider || !withSliderTravelDistance)
                return 0;

            return osuCurrObj.TravelDistance / osuCurrObj.TravelTime;
        }

        private static double calculateCurrentVelocity(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj, double currDistance, bool withSliderTravelDistance)
        {
            double currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;

            // If the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderDistance = osuLastObj.LazyTravelDistance + osuCurrObj.LazyJumpDistance;
                currVelocity = Math.Max(currVelocity, sliderDistance / osuCurrObj.AdjustedDeltaTime);
            }

            return currVelocity;
        }

        /// <summary>
        /// Reduces bonuses when the current and previous two objects all overlap, as no additional movement is required.
        /// </summary>
        private static double calculateOverlappedNotesWeight(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj)
        {
            if (osuCurrObj.Index <= 2)
                return 1;

            var osuLastLastObj = (OsuDifficultyHitObject)osuCurrObj.Previous(1);

            double o1 = calculateOverlapFactor(osuCurrObj, osuLastObj);
            double o2 = calculateOverlapFactor(osuCurrObj, osuLastLastObj);
            double o3 = calculateOverlapFactor(osuLastObj, osuLastLastObj);

            return 1 - o1 * o2 * o3;
        }

        private static double calculateOverlapFactor(OsuDifficultyHitObject first, OsuDifficultyHitObject second)
        {
            var firstBase = (OsuHitObject)first.BaseObject;
            var secondBase = (OsuHitObject)second.BaseObject;
            double objectRadius = firstBase.Radius;

            double distance = Vector2.Distance(firstBase.StackedPosition, secondBase.StackedPosition);
            return Math.Clamp(1 - DiffUtils.Pow(Math.Max(distance - objectRadius, 0) / objectRadius, 2), 0, 1);
        }
    }
}
