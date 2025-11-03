// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class AimEvaluator
    {
        private const double wide_angle_multiplier = 1.5;
        private const double acute_angle_multiplier = 2.24;
        private const double velocity_change_multiplier = 0.75;
        private const double wiggle_multiplier = 1.02;

        public const double SLIDER_MULTIPLIER = 1.27;

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

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);
            var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);

            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Start from snapping difficulty
            double currDistance = calculateSnappingDifficulty(osuCurrObj.LazyJumpDistance, osuCurrObj, osuLastObj);

            // Add the distance to current object and find the velocity
            currDistance += osuCurrObj.LazyJumpDistance;
            double currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLastObj.TravelDistance / osuLastObj.TravelTime; // calculate the slider velocity from slider head to slider end.

                // Need to consider snapping difficulty here as well
                double movement = calculateSnappingDifficulty(osuCurrObj.MinimumJumpDistance, osuCurrObj, osuLastObj);

                movement += osuCurrObj.MinimumJumpDistance;
                double movementVelocity = movement / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            // In previous velocity calculation accounting for snapping difficulty is not needed, as it's not used as a difficulty base.
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.AdjustedDeltaTime;

            if (osuLastLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLastLastObj.TravelDistance / osuLastLastObj.TravelTime;
                double movementVelocity = osuLastObj.MinimumJumpDistance / osuLastObj.MinimumJumpTime;

                prevVelocity = Math.Max(prevVelocity, movementVelocity + travelVelocity);
            }

            double wideAngleBonus = 0;
            double acuteAngleBonus = 0;
            double sliderBonus = 0;
            double velocityChangeBonus = 0;
            double wiggleBonus = 0;

            double aimStrain = currVelocity; // Start strain with regular velocity.

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
            {
                double currAngle = osuCurrObj.Angle.Value;
                double lastAngle = osuLastObj.Angle.Value;

                // Rewarding angles, take the smaller velocity as base.
                double angleBonus = Math.Min(currVelocity, prevVelocity);

                if (Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) < 1.25 * Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime)) // If rhythms are the same.
                {
                    acuteAngleBonus = CalcAcuteAngleBonus(currAngle);

                    // Penalize angle repetition.
                    acuteAngleBonus *= 0.08 + 0.66 * (1 - Math.Min(acuteAngleBonus, Math.Pow(CalcAcuteAngleBonus(lastAngle), 3)));

                    // Apply acute angle bonus for BPM above 300 1/2 and distance more than one diameter
                    acuteAngleBonus *= angleBonus *
                                       DifficultyCalculationUtils.Smootherstep(DifficultyCalculationUtils.MillisecondsToBPM(osuCurrObj.AdjustedDeltaTime, 2), 300, 400);
                }

                // Rescale wide angle bonus to reward lower spacing more
                double velocityThreshold = diameter * 2.3 / osuCurrObj.AdjustedDeltaTime;
                double wideVelocityBase = Math.Min(angleBonus, velocityThreshold + 0.4 * (angleBonus - velocityThreshold));

                wideAngleBonus = wideVelocityBase * calcWideAngleBonus(currAngle);

                // Penalize angle repetition.
                wideAngleBonus *= 1 - Math.Min(wideAngleBonus, Math.Pow(calcWideAngleBonus(lastAngle), 3));

                // Apply wiggle bonus for jumps that are [radius, 3*diameter] in distance, with < 110 angle
                // https://www.desmos.com/calculator/dp0v0nvowc
                wiggleBonus = angleBonus
                              * Math.Pow(DifficultyCalculationUtils.ReverseLerp(currDistance, diameter * 3, diameter), 1.8)
                              * DifficultyCalculationUtils.Smootherstep(currAngle, double.DegreesToRadians(110), double.DegreesToRadians(60))
                              * Math.Pow(DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, diameter * 3, diameter), 1.8)
                              * DifficultyCalculationUtils.Smootherstep(lastAngle, double.DegreesToRadians(110), double.DegreesToRadians(60));

                if (osuLast2Obj != null)
                {
                    // If objects just go back and forth through a middle point - don't give as much wide bonus
                    // Use Previous(2) and Previous(0) because angles calculation is done prevprev-prev-curr, so any object's angle's center point is always the previous object
                    var lastBaseObject = (OsuHitObject)osuLastObj.BaseObject;
                    var last2BaseObject = (OsuHitObject)osuLast2Obj.BaseObject;

                    float distance = (last2BaseObject.StackedPosition - lastBaseObject.StackedPosition).Length;

                    if (distance < 1)
                    {
                        wideAngleBonus *= 1 - 0.35 * (1 - distance);
                    }
                }
            }

            // We want to use the average velocity over the whole object when awarding differences, not the individual jump and slider path velocities.
            prevVelocity = (osuLastObj.LazyJumpDistance + osuLastLastObj.TravelDistance) / osuLastObj.AdjustedDeltaTime;
            currVelocity = (osuCurrObj.LazyJumpDistance + osuLastObj.TravelDistance) / osuCurrObj.AdjustedDeltaTime;

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = DifficultyCalculationUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                velocityChangeBonus *= Math.Pow(Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) / Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), 2);
            }

            if (osuLastObj.BaseObject is Slider)
            {
                // Reward sliders based on velocity.
                sliderBonus = osuLastObj.TravelDistance / osuLastObj.TravelTime;
            }

            aimStrain += wiggleBonus * wiggle_multiplier;
            aimStrain += velocityChangeBonus * velocity_change_multiplier;

            // Add in acute angle bonus or wide angle bonus, whichever is larger.
            aimStrain += Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier);

            // Apply high circle size bonus
            aimStrain *= osuCurrObj.SmallCircleBonus;

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance)
                aimStrain += sliderBonus * SLIDER_MULTIPLIER;

            return aimStrain;
        }

        private static double calculateSnappingDifficulty(double currDistance, OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj)
        {
            const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Additional reward for wide angles being hard to snap on high BPM
            double angleSnapDifficultyBonus = 0;
            double deltaTimeThreshold = DifficultyCalculationUtils.BPMToMilliseconds(180, 2);

            if (osuCurrObj.AdjustedDeltaTime < deltaTimeThreshold)
            {
                double bpmFactor = Math.Pow((deltaTimeThreshold - osuCurrObj.AdjustedDeltaTime) * 0.015, 2.5);

                angleSnapDifficultyBonus = diameter * bpmFactor;

                // We want to start reward from 60 degrees to 90 degrees on lower spacing, and form 90 degrees to 120 degrees on higher spacing
                double highSpacingAdjust = Math.PI / 6;
                highSpacingAdjust *= DifficultyCalculationUtils.ReverseLerp(currDistance, diameter * 2, diameter * 4);

                angleSnapDifficultyBonus *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.Angle ?? 0, Math.PI / 3 + highSpacingAdjust, Math.PI / 2 + highSpacingAdjust);
            }

            double bpm = DifficultyCalculationUtils.BPMToMilliseconds(osuCurrObj.AdjustedDeltaTime, 2);
            double snapThreshold = diameter * (1 + 1.3 * DifficultyCalculationUtils.ReverseLerp(bpm, 200, 250));

            // Jumps need to have some spacing to be snapped
            double distanceSnapDifficultyBonus = currDistance < snapThreshold ? (snapThreshold * 0.65 + currDistance * 0.35) - currDistance : 0;

            // Don't buff doubles jumps as you don't snap in this case (except very close to itself doubles, that need to have some distance bonus to be calculated as flow)
            double lowSpacingFactor = DifficultyCalculationUtils.ReverseLerp(currDistance, radius * 2, radius);

            // Make nerf much smaller if it's not doubles
            double notOverlappingAdjust = diameter * 2 * (1 - lowSpacingFactor);

            // Don't increase snap distance when previous jump is very big, as it leads to cheese being overrewarded
            double bigDistanceDifferenceFactor = DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, notOverlappingAdjust + diameter, notOverlappingAdjust + diameter * 2);

            // And don't nerf bursts with this
            bigDistanceDifferenceFactor *= DifficultyCalculationUtils.ReverseLerpTwoDirectional(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime, 1.95, 1.5);

            return (distanceSnapDifficultyBonus + angleSnapDifficultyBonus) * (1 - bigDistanceDifferenceFactor);
        }

        private static double calcWideAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(40), double.DegreesToRadians(140));
        public static double CalcAcuteAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(140), double.DegreesToRadians(40));
    }
}
