// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class SnapAimEvaluator
    {
        /// <summary>
        /// Evaluates the difficulty of aiming the current object, based on:
        /// <list type="bullet">
        /// <item><description>cursor velocity to the current object,</description></item>
        /// <item><description>angle difficulty,</description></item>
        /// <item><description>sharp velocity increases,</description></item>
        /// <item><description>and slider difficulty.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.Index <= 1 || current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous();

            double currDistance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double currVelocity = calculateCurrentVelocity(osuCurrObj, osuLastObj, currDistance, withSliderTravelDistance);

            double prevDistance = withSliderTravelDistance ? osuLastObj.LazyJumpDistance : osuLastObj.JumpDistance;
            double prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;

            double snapDifficulty = currVelocity; // Start difficulty with regular velocity.

            // Penalize angle repetition.
            snapDifficulty *= vectorAngleRepetition(osuCurrObj, osuLastObj);
            snapDifficulty += calculateWideAngleBonus(osuCurrObj, osuLastObj, currDistance, prevDistance, withSliderTravelDistance);
            snapDifficulty += calculateVelocityChangeBonus(withSliderTravelDistance, prevVelocity, currVelocity, currDistance, osuCurrObj, osuLastObj);

            if (osuCurrObj.BaseObject is Slider && withSliderTravelDistance)
            {
                snapDifficulty += calculateSliderBonus(osuCurrObj);
            }

            // Apply high circle size bonus
            snapDifficulty *= osuCurrObj.SmallCircleBonus;

            snapDifficulty *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            return snapDifficulty;
        }

        private static double calculateWideAngleBonus(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj,
                                                      double currDistance, double prevDistance, bool withSliderTravelDistance)
        {
            const double wide_angle_multiplier = 2.15;

            if (osuCurrObj.Angle == null || osuLastObj.Angle == null)
                return 0;

            double wideAngleBonus = AngleUtils.CalculateWideness(osuCurrObj.Angle.Value);

            // Penalize angle repetition. It is important to do it _before_ multiplying by velocity because we compare raw wideness here
            wideAngleBonus *= 0.25 + 0.75 * (1 - Math.Min(wideAngleBonus, DiffUtils.Pow(AngleUtils.CalculateWideness(osuLastObj.Angle.Value), 3)));

            // Rescaling velocity for the wide angle bonus
            const double wide_angle_time_scale = 1.2;

            double currRescaledVelocity = currDistance / DiffUtils.Pow(osuCurrObj.AdjustedDeltaTime, wide_angle_time_scale);
            double prevRescaledVelocity = prevDistance / DiffUtils.Pow(osuLastObj.AdjustedDeltaTime, wide_angle_time_scale);

            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderDistance = osuLastObj.LazyTravelDistance + osuCurrObj.LazyJumpDistance;
                currRescaledVelocity = Math.Max(currRescaledVelocity, sliderDistance / DiffUtils.Pow(osuCurrObj.AdjustedDeltaTime, wide_angle_time_scale));
            }

            wideAngleBonus *= Math.Min(currRescaledVelocity, prevRescaledVelocity);

            var osuLast2Obj = (OsuDifficultyHitObject)osuCurrObj.Previous(2);

            if (osuLast2Obj != null)
            {
                // If objects just go back and forth through a middle point - don't give as much wide bonus
                // Use Previous(2) and Previous(0) because angles calculation is done prevprev-prev-curr, so any object's angle's center point is always the previous object
                var lastBaseObject = (OsuHitObject)osuLastObj.BaseObject;
                var last2BaseObject = (OsuHitObject)osuLast2Obj.BaseObject;

                float distance = (last2BaseObject.StackedPosition - lastBaseObject.StackedPosition).Length;

                if (distance < 1)
                {
                    wideAngleBonus *= 1 - 0.55 * (1 - distance);
                }
            }

            return wideAngleBonus * wide_angle_multiplier;
        }

        private static double calculateVelocityChangeBonus(bool withSliderTravelDistance, double prevVelocity, double currVelocity,
                                                           double currDistance, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj)
        {
            const double velocity_change_multiplier = 0.85;

            if (Math.Max(prevVelocity, currVelocity) == 0)
                return 0;

            if (withSliderTravelDistance)
            {
                // We want to use just the object jump without slider velocity when awarding differences
                currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;
            }

            // cap velocity to at least 1 radius distance to only award snap patterns
            currVelocity = Math.Max(currVelocity, OsuDifficultyHitObject.NORMALISED_DIAMETER / osuCurrObj.AdjustedDeltaTime);
            prevVelocity = Math.Max(prevVelocity, OsuDifficultyHitObject.NORMALISED_DIAMETER / osuLastObj.AdjustedDeltaTime);

            // Scale with ratio of difference compared to 0.5 * max dist.
            double distRatio = DiffUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

            // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
            double overlapVelocityBuff = Math.Min(OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), Math.Abs(prevVelocity - currVelocity));

            double velocityChangeBonus = overlapVelocityBuff * distRatio;

            // Penalize for rhythm changes.
            velocityChangeBonus *= DiffUtils.Pow(Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) / Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), 3);

            return velocityChangeBonus * velocity_change_multiplier;
        }

        private static double calculateSliderBonus(OsuDifficultyHitObject osuCurrObj)
        {
            const double slider_multiplier = 1.35;

            // Reward sliders based on velocity.
            double sliderBonus = osuCurrObj.TravelDistance / osuCurrObj.TravelTime;
            double rescaledSliderBonus = sliderBonus < 1 ? sliderBonus : DiffUtils.Pow(sliderBonus, 0.75);

            return rescaledSliderBonus * slider_multiplier;
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

        private static double vectorAngleRepetition(OsuDifficultyHitObject current, OsuDifficultyHitObject previous)
        {
            if (
                current.Angle == null ||
                previous.Angle == null ||
                previous.BaseObject is Spinner ||
                previous.Previous(0).BaseObject is Spinner)
                return 1;

            const double note_limit = 6;
            const double maximum_repetition_nerf = 0.15;
            const double maximum_vector_influence = 0.5;

            double constantAngleCount = 0;

            for (int index = 0; index < note_limit; index++)
            {
                OsuDifficultyHitObject? prevObj = (OsuDifficultyHitObject)current.Previous(index);

                if (prevObj == null || prevObj.BaseObject is Spinner)
                    break;

                // Only consider vectors in the same jump section, stopping to change rhythm ruins momentum
                if (Math.Max(current.AdjustedDeltaTime, prevObj.AdjustedDeltaTime) > 1.1 * Math.Min(current.AdjustedDeltaTime, prevObj.AdjustedDeltaTime))
                    break;

                if (prevObj.NormalisedVectorAngle == null || current.NormalisedVectorAngle == null)
                    continue;

                double angleDifference = Math.Abs(current.NormalisedVectorAngle.Value - prevObj.NormalisedVectorAngle.Value);

                // Refer to this desmos for tuning, constants need to be precise so that values stay within the range of 0 and 1.
                // https://www.desmos.com/calculator/a8jesv5sv2
                constantAngleCount += Math.Cos(8 * Math.Min(double.DegreesToRadians(11.25), angleDifference));
            }

            double vectorRepetition = DiffUtils.Pow(Math.Min(0.5 / constantAngleCount, 1), 2);

            double stackFactor = DiffUtils.Smootherstep(current.LazyJumpDistance, 0, OsuDifficultyHitObject.NORMALISED_DIAMETER);

            double currAngle = current.Angle.Value;
            double lastAngle = previous.Angle.Value;

            double angleDifferenceAdjusted = Math.Cos(2 * Math.Min(double.DegreesToRadians(45), Math.Abs(currAngle - lastAngle) * stackFactor));

            double baseNerf = 1 - maximum_repetition_nerf * AngleUtils.CalculateAcuteness(lastAngle) * angleDifferenceAdjusted;

            return DiffUtils.Pow(baseNerf + (1 - baseNerf) * vectorRepetition * maximum_vector_influence * stackFactor, 2);
        }

        private static double highBpmBonus(double ms) => 1 / (1 - DiffUtils.Pow(0.03, DiffUtils.Pow(ms / 1000, 0.65)));
    }
}
