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
        private const double slider_multiplier = 1.35;
        private const double velocity_change_multiplier = 0.75;

        private const double slider_shape_reading_multiplier = 1.6;
        private const double slider_end_complexity_multiplier = 5.0;
        private const double slider_end_distance_multiplier = 0.3;

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

            double aimStrain = currVelocity; // Start strain with regular velocity.

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

            if (osuLastObj.BaseObject is Slider slider)
            {
                // Reward sliders based on velocity.
                sliderBonus = osuLastObj.TravelDistance / osuLastObj.TravelTime;

                double readingBonus = 1.0;
                readingBonus += calculateSliderShapeReadingDifficulty(slider) * slider_shape_reading_multiplier;
                readingBonus += calculateSliderEndComplexityDifficulty(slider) * slider_end_complexity_multiplier;
                readingBonus += calculateSliderEndDistanceDifficulty(slider) * slider_end_distance_multiplier;
                sliderBonus *= Math.Max(readingBonus, 1.0);
            }

            // Add in acute angle bonus or wide angle bonus + velocity change bonus, whichever is larger.
            aimStrain += Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier + velocityChangeBonus * velocity_change_multiplier);

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance)
                aimStrain += sliderBonus * slider_multiplier;

            return aimStrain;
        }

        private const int normalized_radius = 50;
        private static double calculateSliderShapeReadingDifficulty(Slider slider)
        {
            double result = 0;

            var followPath = slider.NestedHitObjects;

            double minFollowRadius = slider.Radius;
            double maxFollowRadius = slider.Radius * 2.4;
            double deltaFollowRadius = maxFollowRadius - minFollowRadius;

            int nestedObjectIndex = 0;
            double numberOfUpdates = slider.Path.Distance / slider.Radius;
            double deltaT = slider.SpanDuration / numberOfUpdates;

            for (double relativeTime = 0; relativeTime <= slider.SpanDuration; relativeTime += deltaT)
            {
                double absoluteTime = relativeTime + slider.StartTime;

                while (nestedObjectIndex < followPath.Count - 2 && followPath[nestedObjectIndex + 1].StartTime < absoluteTime)
                {
                    nestedObjectIndex += 1; // search for right lazy path segment
                }

                var currentObject = (OsuHitObject)followPath[nestedObjectIndex];
                var nextObject = (OsuHitObject)followPath[nestedObjectIndex + 1];
                double nextObjectStartTime = nestedObjectIndex == followPath.Count - 2 ? slider.EndTime : nextObject.StartTime;

                // calculating position of the normal path
                double progress = relativeTime / slider.SpanDuration;
                Vector2 ballPosition = slider.Position + slider.Path.PositionAt(progress);

                // calculation position of the lazy path
                float localProgress = (float)((absoluteTime - currentObject.StartTime) / (nextObjectStartTime - currentObject.StartTime));
                localProgress = Math.Clamp(localProgress, 0, 1);
                Vector2 lazyPosition = currentObject.Position + (nextObject.Position - currentObject.Position) * localProgress; // interpolation

                // buff scales from 0 to 1 when slider follow distance is changing from 1.0x to 2.4x
                double continousBuff = (Vector2.Distance(ballPosition, lazyPosition) - minFollowRadius) / deltaFollowRadius;
                continousBuff = Math.Clamp(continousBuff, 0, 1) * deltaT;
                result += (float)continousBuff;
            }

            if (slider.SpanDuration == 0) return 0;
            return (float)(result / slider.SpanDuration);
        }

        private const float short_sliders_penalty = normalized_radius * 0.0f;
        private static double calculateSliderEndComplexityDifficulty(Slider slider)
        {
            if (slider.LazyEndPosition is null) return 0;
            if (slider.LineLazyEndPosition is null) return 0;
            if (slider.LazyTravelDistance == 0) return 0;

            double complexityDistance = Vector2.Distance((Vector2)slider.LineLazyEndPosition, (Vector2)slider.LazyEndPosition);
            complexityDistance = Math.Max(complexityDistance - slider.Radius * 0.5, 0); // prevent slightly curvy sliders from overbuffing

            return complexityDistance / (slider.LazyTravelDistance + short_sliders_penalty);
        }

        private static double calculateSliderEndDistanceDifficulty(Slider slider)
        {
            if (slider.LazyEndPosition is null) return 0.0f;
            if (slider.LazyTravelDistance == 0) return 0;

            float visualDistance = Vector2.Distance(slider.StackedEndPosition, (Vector2)slider.LazyEndPosition);

            var preLastObj = (OsuHitObject)slider.NestedHitObjects[^2];

            double minimalMovement = Vector2.Distance((Vector2)slider.LazyEndPosition, preLastObj.Position) - slider.Radius * 4.8;
            visualDistance *= (float)Math.Clamp(minimalMovement / slider.Radius, 0, 1); // buff only very long sliders
            return visualDistance / (slider.LazyTravelDistance + short_sliders_penalty);
        }

        private static double calcWideAngleBonus(double angle) => Math.Pow(Math.Sin(3.0 / 4 * (Math.Min(5.0 / 6 * Math.PI, Math.Max(Math.PI / 6, angle)) - Math.PI / 6)), 2);

        private static double calcAcuteAngleBonus(double angle) => 1 - calcWideAngleBonus(angle);
    }
}
