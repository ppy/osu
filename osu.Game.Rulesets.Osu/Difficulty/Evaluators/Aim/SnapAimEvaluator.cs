// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
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
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            const double wide_angle_multiplier = 9.67;
            const double acute_angle_multiplier = 2.41;
            const double slider_multiplier = 1.5;
            const double velocity_change_multiplier = 0.9;

            // WARNING: Increasing this multiplier beyond 1.02 reduces difficulty as distance increases. Refer to the desmos link above the wiggle bonus calculation
            const double wiggle_multiplier = 1.02;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);

            const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Calculate the velocity to the current hitobject, which starts with a base distance / time assuming the last object is a hitcircle.
            double currDistance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderDistance = osuLastObj.LazyTravelDistance + osuCurrObj.LazyJumpDistance;
                currVelocity = Math.Max(currVelocity, sliderDistance / osuCurrObj.AdjustedDeltaTime);
            }

            double prevDistance = withSliderTravelDistance ? osuLastObj.LazyJumpDistance : osuLastObj.JumpDistance;
            double prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;

            double snapDifficulty = currVelocity; // Start difficulty with regular velocity.

            // Penalize angle repetition.
            snapDifficulty *= vectorAngleRepetition(osuCurrObj, osuLastObj);

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
            {
                double currAngle = osuCurrObj.Angle.Value;
                double lastAngle = osuLastObj.Angle.Value;

                // Rewarding angles, take the smaller velocity as base.
                double velocityInfluence = Math.Min(currVelocity, prevVelocity);

                double acuteAngleBonus = 0;

                if (Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) < 1.25 * Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime)) // If rhythms are the same.
                {
                    acuteAngleBonus = CalcAngleAcuteness(currAngle);

                    // Penalize angle repetition. It is important to do it _before_ multiplying by anything because we compare raw acuteness here
                    acuteAngleBonus *= 0.08 + 0.92 * (1 - Math.Min(acuteAngleBonus, DiffUtils.Pow(CalcAngleAcuteness(lastAngle), 3)));

                    // Apply acute angle bonus for BPM above 300 1/2 and distance more than one diameter
                    acuteAngleBonus *= velocityInfluence * DiffUtils.Smootherstep(DiffUtils.MillisecondsToBPM(osuCurrObj.AdjustedDeltaTime, 2), 300, 400) *
                                       DiffUtils.Smootherstep(currDistance, 0, diameter * 2);
                }

                double wideAngleBonus = calcAngleWideness(currAngle);

                // Penalize angle repetition. It is important to do it _before_ multiplying by velocity because we compare raw wideness here
                wideAngleBonus *= 0.25 + 0.75 * (1 - Math.Min(wideAngleBonus, DiffUtils.Pow(calcAngleWideness(lastAngle), 3)));

                // Rescaling velocity for the wide angle bonus
                const double wide_angle_time_scale = 1.45;
                double wideAngleCurrVelocity = currDistance / DiffUtils.Pow(osuCurrObj.AdjustedDeltaTime, wide_angle_time_scale);
                double wideAnglePrevVelocity = prevDistance / DiffUtils.Pow(osuLastObj.AdjustedDeltaTime, wide_angle_time_scale);

                if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
                {
                    double sliderDistance = osuLastObj.LazyTravelDistance + osuCurrObj.LazyJumpDistance;
                    wideAngleCurrVelocity = Math.Max(wideAngleCurrVelocity, sliderDistance / DiffUtils.Pow(osuCurrObj.AdjustedDeltaTime, wide_angle_time_scale));
                }

                wideAngleBonus *= Math.Min(wideAngleCurrVelocity, wideAnglePrevVelocity);

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

                // Add in acute angle bonus or wide angle bonus, whichever is larger.
                snapDifficulty += Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier);

                // Apply wiggle bonus for jumps that are [radius, 3*diameter] in distance, with < 110 angle
                // https://www.desmos.com/calculator/dp0v0nvowc
                double wiggleBonus = velocityInfluence
                                     * DiffUtils.Smootherstep(currDistance, radius, diameter)
                                     * DiffUtils.Pow(DiffUtils.ReverseLerp(currDistance, diameter * 3, diameter), 1.8)
                                     * DiffUtils.Smootherstep(currAngle, double.DegreesToRadians(110), double.DegreesToRadians(60))
                                     * DiffUtils.Smootherstep(prevDistance, radius, diameter)
                                     * DiffUtils.Pow(DiffUtils.ReverseLerp(prevDistance, diameter * 3, diameter), 1.8)
                                     * DiffUtils.Smootherstep(lastAngle, double.DegreesToRadians(110), double.DegreesToRadians(60));

                snapDifficulty += wiggleBonus * wiggle_multiplier;
            }

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                if (withSliderTravelDistance)
                {
                    // We want to use just the object jump without slider velocity when awarding differences
                    currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;
                }

                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = DiffUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), Math.Abs(prevVelocity - currVelocity));

                double velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                velocityChangeBonus *= DiffUtils.Pow(Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) / Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), 2);

                snapDifficulty += velocityChangeBonus * velocity_change_multiplier;
            }

            // Reward sliders based on velocity.
            if (osuCurrObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderBonus = osuCurrObj.TravelDistance / osuCurrObj.TravelTime;
                snapDifficulty += (sliderBonus < 1 ? sliderBonus : DiffUtils.Pow(sliderBonus, 0.75)) * slider_multiplier;
            }

            // Apply high circle size bonus
            snapDifficulty *= osuCurrObj.SmallCircleBonus;

            snapDifficulty *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            return snapDifficulty;
        }

        private static double highBpmBonus(double ms) => 1 / (1 - DiffUtils.Pow(0.03, DiffUtils.Pow(ms / 1000, 0.65)));

        private static double vectorAngleRepetition(OsuDifficultyHitObject current, OsuDifficultyHitObject previous)
        {
            if (current.Angle == null || previous.Angle == null)
                return 1;

            const double note_limit = 6;
            const double maximum_repetition_nerf = 0.15;
            const double maximum_vector_influence = 0.5;

            double constantAngleCount = 0;

            for (int index = 0; index < note_limit; index++)
            {
                OsuDifficultyHitObject? prevObj = (OsuDifficultyHitObject)current.Previous(index);

                if (prevObj == null)
                    break;

                // Only consider vectors in the same jump section, stopping to change rhythm ruins momentum
                if (Math.Max(current.AdjustedDeltaTime, prevObj.AdjustedDeltaTime) > 1.1 * Math.Min(current.AdjustedDeltaTime, prevObj.AdjustedDeltaTime))
                    break;

                if (prevObj.NormalisedVectorAngle != null && current.NormalisedVectorAngle != null)
                {
                    double angleDifference = Math.Abs(current.NormalisedVectorAngle.Value - prevObj.NormalisedVectorAngle.Value);
                    // Refer to this desmos for tuning, constants need to be precise so that values stay within the range of 0 and 1.
                    // https://www.desmos.com/calculator/a8jesv5sv2
                    constantAngleCount += Math.Cos(8 * Math.Min(double.DegreesToRadians(11.25), angleDifference));
                }
            }

            double vectorRepetition = DiffUtils.Pow(Math.Min(0.5 / constantAngleCount, 1), 2);

            double stackFactor = DiffUtils.Smootherstep(current.LazyJumpDistance, 0, OsuDifficultyHitObject.NORMALISED_DIAMETER);

            double currAngle = current.Angle.Value;
            double lastAngle = previous.Angle.Value;

            double angleDifferenceAdjusted = Math.Cos(2 * Math.Min(double.DegreesToRadians(45), Math.Abs(currAngle - lastAngle) * stackFactor));

            double baseNerf = 1 - maximum_repetition_nerf * CalcAngleAcuteness(lastAngle) * angleDifferenceAdjusted;

            return DiffUtils.Pow(baseNerf + (1 - baseNerf) * vectorRepetition * maximum_vector_influence * stackFactor, 2);
        }

        private static double calcAngleWideness(double angle) => DiffUtils.Smoothstep(angle, double.DegreesToRadians(40), double.DegreesToRadians(140));

        public static double CalcAngleAcuteness(double angle) => DiffUtils.Smoothstep(angle, double.DegreesToRadians(140), double.DegreesToRadians(40));
    }
}
