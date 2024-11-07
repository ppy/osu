﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class FlashlightEvaluator
    {
        private const double max_opacity_bonus = 0.4;
        private const double hidden_bonus = 0.2;

        private const double min_velocity = 0.5;
        private const double max_velocity = 1.5;
        private const double slider_multiplier = 0.3;

        private const double min_angle_multiplier = 0.2;

        /// <summary>
        /// Evaluates the difficulty of memorising and hitting an object, based on:
        /// <list type="bullet">
        /// <item><description>distance between a number of previous objects and the current object,</description></item>
        /// <item><description>the visual opacity of the current object,</description></item>
        /// <item><description>the angle made by the current object,</description></item>
        /// <item><description>length and speed of the current object (for sliders),</description></item>
        /// <item><description>and whether the hidden mod is enabled.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden, OsuModFlashlight osuModFlashlight)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            var osuHitObject = (OsuHitObject)(osuCurrent.BaseObject);

            double scalingFactor = 52.0 / osuHitObject.Radius;
            double smallDistNerf = 1.0;
            double cumulativeStrainTime = 0.0;

            double result = 0.0;

            OsuDifficultyHitObject lastObj = osuCurrent;

            double angleRepeatCount = 0.0;

            // This is iterating backwards in time from the current object.
            for (int i = 0; i < Math.Min(current.Index, 10); i++)
            {
                var currentObj = (OsuDifficultyHitObject)current.Previous(i);
                var currentHitObject = (OsuHitObject)(currentObj.BaseObject);

                cumulativeStrainTime += lastObj.StrainTime;

                if (!(currentObj.BaseObject is Spinner))
                {
                    double pixelDistance = (osuHitObject.StackedPosition - currentHitObject.StackedEndPosition).Length;
                    double flashlightRadius = getSize(currentObj.CurrentMaxCombo, osuModFlashlight);
                    double objectOpacity = osuCurrent.OpacityAt(currentHitObject.StartTime, hidden);

                    // Consider the jump from the lazy end position for sliders.
                    if (currentHitObject is Slider currentSlider)
                    {
                        Vector2 lazyEndPosition = currentSlider.LazyEndPosition ?? currentSlider.StackedPosition;
                        pixelDistance = Math.Min(pixelDistance, (osuHitObject.StackedPosition - lazyEndPosition).Length);
                    }

                    // Apply a nerf based on the visibility from the current object.
                    double radiusVisibility = Math.Min(1.0, pixelDistance / (flashlightRadius + osuHitObject.Radius - 80));
                    double visibilityNerf = 1.0 - objectOpacity * (1.0 - radiusVisibility);

                    // Small jumps within the visible Flashlight radius should be nerfed.
                    if (i == 0)
                        smallDistNerf = Math.Min(1.0, pixelDistance / (flashlightRadius - 45));

                    // Nerf stacks so that only the first object of the stack is accounted for.
                    double stackNerf = Math.Min(1.0, (currentObj.LazyJumpDistance / scalingFactor) / 25.0);

                    // Bonus based on object opacity.
                    double opacityBonus = 1.0 + max_opacity_bonus * (1.0 - objectOpacity);

                    result += visibilityNerf * stackNerf * opacityBonus * scalingFactor * pixelDistance / cumulativeStrainTime;

                    if (currentObj.Angle != null && osuCurrent.Angle != null)
                    {
                        // Objects further back in time should count less for the nerf.
                        if (Math.Abs(currentObj.Angle.Value - osuCurrent.Angle.Value) < 0.02)
                            angleRepeatCount += Math.Max(1.0 - 0.1 * i, 0.0);
                    }
                }

                lastObj = currentObj;
            }

            result = Math.Pow(smallDistNerf * result, 2.0);

            // Additional bonus for Hidden due to there being no approach circles.
            if (hidden)
                result *= 1.0 + hidden_bonus;

            // Nerf patterns with repeated angles.
            result *= min_angle_multiplier + (1.0 - min_angle_multiplier) / (angleRepeatCount + 1.0);

            double sliderBonus = 0.0;

            if (osuCurrent.BaseObject is Slider osuSlider)
            {
                // Invert the scaling factor to determine the true travel distance independent of circle size.
                double pixelTravelDistance = osuSlider.LazyTravelDistance / scalingFactor;

                // Reward sliders based on cursor velocity.
                sliderBonus = Math.Log(pixelTravelDistance / osuCurrent.TravelTime + 1);

                // More cursor movement requires more memorisation.
                sliderBonus *= osuSlider.LazyTravelDistance;

                // Nerf slow slider velocity.
                double sliderVelocity = osuSlider.Distance / osuCurrent.TravelTime;
                sliderBonus *= Math.Clamp((sliderVelocity - min_velocity) / (max_velocity - min_velocity), 0, 1);

                // Nerf sliders the more repeats they have, as less memorisation is required.
                sliderBonus /= 0.75 * osuSlider.RepeatCount + 1;
            }

            result += Math.Pow(sliderBonus, 1.2) * slider_multiplier;

            return result;
        }

        private static float getSize(int combo, OsuModFlashlight osuModFlashlight)
        {
            float size = osuModFlashlight.DefaultFlashlightSize * osuModFlashlight.SizeMultiplier.Value;

            if (osuModFlashlight.ComboBasedSize.Value)
                size *= getComboScaleFor(combo);

            return size;
        }

        private static float getComboScaleFor(int combo)
        {
            if (combo >= 200)
                return 0.625f;
            if (combo >= 100)
                return 0.8125f;

            return 1.0f;
        }
    }
}
