﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        private const double acute_angle_multiplier = 2.7;
        private const double slider_multiplier = 1.35;
        private const double velocity_change_multiplier = 0.75;
        private const double wiggle_multiplier = 1.02;

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

            const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Calculate the velocity to the current hitobject, which starts with a base distance / time assuming the last object is a hitcircle.
            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLastObj.TravelDistance / osuLastObj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            // As above, do the same for the previous hitobject.
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

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

            if (Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime) < 1.25 * Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime)) // If rhythms are the same.
            {
                if (osuCurrObj.Angle != null && osuLastObj.Angle != null && osuLastLastObj.Angle != null)
                {
                    double currAngle = osuCurrObj.Angle.Value;
                    double lastAngle = osuLastObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    double angleBonus = Math.Min(currVelocity, prevVelocity);

                    wideAngleBonus = calcWideAngleBonus(currAngle);
                    acuteAngleBonus = calcAcuteAngleBonus(currAngle);

                    // Penalize angle repetition.
                    wideAngleBonus *= 1 - Math.Min(wideAngleBonus, Math.Pow(calcWideAngleBonus(lastAngle), 3));
                    acuteAngleBonus *= 0.03 + 0.97 * (1 - Math.Min(acuteAngleBonus, Math.Pow(calcAcuteAngleBonus(lastAngle), 3)));

                    // Apply full wide angle bonus for distance more than one diameter
                    wideAngleBonus *= angleBonus * DifficultyCalculationUtils.Smootherstep(osuCurrObj.LazyJumpDistance, 0, diameter);

                    // Apply acute angle bonus for BPM above 300 1/2 and distance more than one diameter
                    acuteAngleBonus *= angleBonus *
                                       DifficultyCalculationUtils.Smootherstep(DifficultyCalculationUtils.MillisecondsToBPM(osuCurrObj.StrainTime, 2), 300, 400) *
                                       DifficultyCalculationUtils.Smootherstep(osuCurrObj.LazyJumpDistance, diameter, diameter * 2);

                    // Apply wiggle bonus for jumps that are [radius, 3*diameter] in distance, with < 110 angle
                    // https://www.desmos.com/calculator/dp0v0nvowc
                    wiggleBonus = angleBonus
                                  * DifficultyCalculationUtils.Smootherstep(osuCurrObj.LazyJumpDistance, radius, diameter)
                                  * Math.Pow(DifficultyCalculationUtils.ReverseLerp(osuCurrObj.LazyJumpDistance, diameter * 3, diameter), 1.8)
                                  * DifficultyCalculationUtils.Smootherstep(currAngle, double.DegreesToRadians(110), double.DegreesToRadians(60))
                                  * DifficultyCalculationUtils.Smootherstep(osuLastObj.LazyJumpDistance, radius, diameter)
                                  * Math.Pow(DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, diameter * 3, diameter), 1.8)
                                  * DifficultyCalculationUtils.Smootherstep(lastAngle, double.DegreesToRadians(110), double.DegreesToRadians(60));
                }
            }

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                // We want to use the average velocity over the whole object when awarding differences, not the individual jump and slider path velocities.
                prevVelocity = (osuLastObj.LazyJumpDistance + osuLastLastObj.TravelDistance) / osuLastObj.StrainTime;
                currVelocity = (osuCurrObj.LazyJumpDistance + osuLastObj.TravelDistance) / osuCurrObj.StrainTime;

                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = Math.Pow(Math.Sin(Math.PI / 2 * Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity)), 2);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime), Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                velocityChangeBonus *= Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2);
            }

            if (osuLastObj.BaseObject is Slider)
            {
                // Reward sliders based on velocity.
                sliderBonus = osuLastObj.TravelDistance / osuLastObj.TravelTime;
            }

            aimStrain += wiggleBonus * wiggle_multiplier;

            // Add in acute angle bonus or wide angle bonus + velocity change bonus, whichever is larger.
            aimStrain += Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier + velocityChangeBonus * velocity_change_multiplier);

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance)
                aimStrain += sliderBonus * slider_multiplier;

            return aimStrain;
        }

        private static double calcWideAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(30), double.DegreesToRadians(150));

        private static double calcAcuteAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(150), double.DegreesToRadians(30));
    }
}
