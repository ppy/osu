// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class AimEvaluator
    {
        private const double wide_angle_multiplier = 1.5;
        private const double acute_angle_multiplier = 1.95;
        private const double velocity_change_multiplier = 0.75;

        private const double slider_aim_multiplier = 2.0;
        private const double slider_jump_multiplier = 0.09;

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

            // Calculate the velocity to the current hitobject, which starts with a base distance / time assuming the last object is a hitcircle.
            double currVelocity = osuCurrObj.JumpDistance / osuCurrObj.StrainTime;

            // As above, do the same for the previous hitobject.
            double prevVelocity = osuLastObj.JumpDistance / osuLastObj.StrainTime;

            double wideAngleBonus = 0;
            double acuteAngleBonus = 0;
            double velocityChangeBonus = 0;

            double aimStrain = currVelocity; // Start strain with regular velocity.

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object
                aimStrain = Math.Max(currVelocity, movementVelocity); // take the larger velocity.
            }

            if (Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime) < 1.25 * Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime)) // If rhythms are the same.
            {
                if (osuCurrObj.Angle != null && osuLastObj.Angle != null && osuLastLastObj.Angle != null)
                {
                    double currAngle = osuCurrObj.Angle.Value;
                    double lastAngle = osuLastObj.Angle.Value;
                    double lastLastAngle = osuLastLastObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    double angleBonus = Math.Min(currVelocity, prevVelocity);

                    wideAngleBonus = calcWideAngleBonus(currAngle);
                    acuteAngleBonus = calcAcuteAngleBonus(currAngle);

                    if (osuCurrObj.StrainTime > 100) // Only buff deltaTime exceeding 300 bpm 1/2.
                        acuteAngleBonus = 0;
                    else
                    {
                        acuteAngleBonus *= calcAcuteAngleBonus(lastAngle) // Multiply by previous angle, we don't want to buff unless this is a wiggle type pattern.
                                           * Math.Min(angleBonus, 125 / osuCurrObj.StrainTime) // The maximum velocity we buff is equal to 125 / strainTime
                                           * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (100 - osuCurrObj.StrainTime) / 25)), 2) // scale buff from 150 bpm 1/4 to 200 bpm 1/4
                                           * Math.Pow(Math.Sin(Math.PI / 2 * (Math.Clamp(osuCurrObj.LazyJumpDistance, 50, 100) - 50) / 50), 2); // Buff distance exceeding 50 (radius) up to 100 (diameter).
                    }

                    // Penalize wide angles if they're repeated, reducing the penalty as the lastAngle gets more acute.
                    wideAngleBonus *= angleBonus * (1 - Math.Min(wideAngleBonus, Math.Pow(calcWideAngleBonus(lastAngle), 3)));
                    // Penalize acute angles if they're repeated, reducing the penalty as the lastLastAngle gets more obtuse.
                    acuteAngleBonus *= 0.5 + 0.5 * (1 - Math.Min(acuteAngleBonus, Math.Pow(calcAcuteAngleBonus(lastLastAngle), 3)));
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
                double overlapVelocityBuff = Math.Min(125 / Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime), Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                velocityChangeBonus *= Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2);
            }

            // Add in acute angle bonus or wide angle bonus + velocity change bonus, whichever is larger.
            aimStrain += Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier + velocityChangeBonus * velocity_change_multiplier);

            double sliderJumpBonus = 0;
            {
                if (osuLastObj.BaseObject is Slider slider)
                {
                    // Take just slider heads into account because we're computing sliderjumps, not slideraim
                    sliderJumpBonus = slider_jump_multiplier * aimStrain;

                    // Reward more if sliders and circles are alternating (actually it's still lower than several sliders in a row)
                    if (osuLastLastObj?.BaseObject is HitCircle)
                    {
                        double alternatingBonus = 0.5 * slider_jump_multiplier * osuLastObj.JumpDistance / osuLastObj.StrainTime;

                        if (osuLastObj.StrainTime > osuLastLastObj.StrainTime)
                            alternatingBonus *= Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2);

                        sliderJumpBonus += alternatingBonus;
                    }

                    // If slider was slower than notes before - punish it
                    if (osuCurrObj.StrainTime > osuLastObj.StrainTime)
                        sliderJumpBonus *= Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2);

                    // Punish too short sliders to prevent cheesing (cheesing is still possible, but it's very rare)
                    double sliderLength = slider.Velocity * slider.SpanDuration;
                    if (sliderLength < slider.Radius)
                        sliderJumpBonus *= sliderLength / slider.Radius;
                }
            }

            aimStrain += sliderJumpBonus;

            // Add in additional slider velocity bonus.
            double sliderBonus = 0;
            {

                if (withSliderTravelDistance && osuLastObj.BaseObject is Slider slider)
                {
                    // Reward sliders based on velocity.
                    sliderBonus = osuLastObj.TravelDistance / osuLastObj.TravelTime;

                    // Bandaid to prevent abuse where aiming difficulty in same direction is added twice
                    double jumpPathSliderNerf = 1.0;

                    // If both of the angles are very wide
                    double sliderBodyReversedAngle = calcAngleBetweenPoints(slider.StackedPosition, (Vector2)slider.LazyEndPosition!, slider.StackedEndPosition);
                    double sliderJumpReversedAngle = calcAngleBetweenPoints((Vector2)slider.LazyEndPosition, slider.StackedEndPosition, ((OsuHitObject)osuCurrObj.BaseObject).StackedPosition);
                    jumpPathSliderNerf *= Math.Pow(calcAcuteAngleBonus(2 * (sliderBodyReversedAngle + sliderJumpReversedAngle)), 2);

                    // If velocities are the same
                    double sliderJumpVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime;
                    double adjustedSliderBodyVelocity = 1.5 * (slider.LazyTravelDistance + slider.Radius) / slider.LazyTravelTime; // Adjusting to account for stopping

                    if (adjustedSliderBodyVelocity > 0)
                    {
                        double velocityDifference = Math.Min(adjustedSliderBodyVelocity, sliderJumpVelocity) / Math.Max(adjustedSliderBodyVelocity, sliderJumpVelocity);
                        jumpPathSliderNerf *= Math.Pow(velocityDifference, 2);
                    }

                    // If duration is half of the jump or lower
                    if (slider.Duration * 2 > osuCurrObj.StrainTime)
                        jumpPathSliderNerf *= 1 - (slider.Duration * 2 - osuCurrObj.StrainTime) / osuCurrObj.StrainTime;

                    // Apply nerf to a slider-aim value
                    sliderBonus *= (1 - jumpPathSliderNerf * 0.9);
                }

            }

            aimStrain += sliderBonus * slider_aim_multiplier;

            return aimStrain;
        }

        private static double calcWideAngleBonus(double angle) => Math.Pow(Math.Sin(3.0 / 4 * (Math.Min(5.0 / 6 * Math.PI, Math.Max(Math.PI / 6, angle)) - Math.PI / 6)), 2);

        private static double calcAcuteAngleBonus(double angle) => 1 - calcWideAngleBonus(angle);

        private static double calcAngleBetweenPoints(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 v1 = p2 - p1;
            Vector2 v2 = p3 - p2;

            float dot = Vector2.Dot(v1, v2);
            float det = v1.X * v2.Y - v1.Y * v2.X;

            return Math.Abs(Math.Atan2(det, dot));
        }
    }
}
