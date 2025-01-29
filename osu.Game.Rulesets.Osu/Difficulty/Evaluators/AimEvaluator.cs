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
        private const double acute_angle_multiplier = 2.6;
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

            var osuNextObj = (OsuDifficultyHitObject)current.Next(0);
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
                if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
                {
                    double currAngle = osuCurrObj.Angle.Value;
                    double lastAngle = osuLastObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    double angleBonus = Math.Min(currVelocity, prevVelocity);

                    wideAngleBonus = calcWideAngleBonus(currAngle);
                    acuteAngleBonus = calcAcuteAngleBonus(currAngle);

                    // Penalize angle repetition.
                    wideAngleBonus *= 1 - Math.Min(wideAngleBonus, Math.Pow(calcWideAngleBonus(lastAngle), 3));
                    acuteAngleBonus *= 0.08 + 0.92 * (1 - Math.Min(acuteAngleBonus, Math.Pow(calcAcuteAngleBonus(lastAngle), 3)));

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

                double relevantStrainTime = Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime);

                // Scale with ratio of difference from 0 on same velocities to 1 on close-to-zero velocity vs X velocity.
                double differenceCoefficient = DifficultyCalculationUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity), 0, Math.Max(prevVelocity, currVelocity));

                // Reward for delta distance up to 1.25 diameters.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / relevantStrainTime, Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * differenceCoefficient;

                // Penalize for rhythm changes.
                double rhythmNerf = Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2);

                // Don't nerf small rhythm changes to avoid nerfing hard rhythms as much as possible
                velocityChangeBonus *= double.Lerp(1, rhythmNerf, DifficultyCalculationUtils.Smoothstep(rhythmNerf, 0.5, 0.3));

                // Apply additional buff for low-spaced doubles.
                if (osuNextObj != null && prevVelocity > 0)
                {
                    double deltaVelocity = Math.Max(Math.Min(prevVelocity, diameter * 1.5 / relevantStrainTime) - currVelocity, 0);

                    // Don't buff big spacing.
                    deltaVelocity *= DifficultyCalculationUtils.ReverseLerp(prevVelocity, diameter * 3 / relevantStrainTime, diameter * 1.5 / relevantStrainTime);

                    // Punish rhythm changes more harshly compared to previous bonus.
                    deltaVelocity *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLastObj.StrainTime * 1.75, osuLastObj.StrainTime * 1.5); // Don't buff burst into jump
                    deltaVelocity *= DifficultyCalculationUtils.Smoothstep(osuLastObj.StrainTime, osuCurrObj.StrainTime * 1.75, osuCurrObj.StrainTime * 1.5); // Don't buff jump to burst
                    deltaVelocity *= DifficultyCalculationUtils.Smoothstep(osuNextObj.StrainTime, osuCurrObj.StrainTime * 1.75, osuCurrObj.StrainTime * 1.5); // Don't buff jump into slider

                    // Have a threshold so very small changes aren't buffed.
                    deltaVelocity *= DifficultyCalculationUtils.Smoothstep(deltaVelocity, 0, (radius / 2.0) / relevantStrainTime);

                    // Increase buff if velocity change is unconventioanl
                    double nextVelocity = (osuNextObj.LazyJumpDistance + osuCurrObj.TravelDistance) / osuNextObj.StrainTime;
                    deltaVelocity *= 1 + 0.5
                        * DifficultyCalculationUtils.ReverseLerp(prevVelocity, 0, nextVelocity * 0.5)
                        * DifficultyCalculationUtils.ReverseLerp(prevVelocity, nextVelocity, nextVelocity * 0.5);

                    // Increase buff if it's difficult/impossible to doubletap.
                    deltaVelocity *= 1 + 0.5 * DifficultyCalculationUtils.Smoothstep(osuCurrObj.LazyJumpDistance, diameter * 0.75, diameter * 1.5);

                    // Nerf low OD as this type of pattern is much easier on low OD
                    deltaVelocity *= 1 - DifficultyCalculationUtils.ReverseLerp(osuCurrObj.HitWindowGreat - 20, relevantStrainTime * 0.5, relevantStrainTime * 0.8)
                        * DifficultyCalculationUtils.Smoothstep(osuCurrObj.LazyJumpDistance, diameter, radius * 0.5);

                    velocityChangeBonus += 1.35 * deltaVelocity;
                }
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

        private static double calcWideAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(40), double.DegreesToRadians(140));

        private static double calcAcuteAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(140), double.DegreesToRadians(40));
    }
}
