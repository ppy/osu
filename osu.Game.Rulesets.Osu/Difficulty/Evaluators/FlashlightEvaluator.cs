// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class FlashlightEvaluator
    {
        private const double max_opacity_bonus = 0.4;
        private const double hidden_bonus = 0.2;

        private const double min_velocity = 0.5;
        private const double slider_multiplier = 1.3;

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
        public static double EvaluateDifficultyOf(OsuDifficultyHitObject currObj, IReadOnlyList<Mod> mods)
        {
            if (currObj.BaseObject is Spinner)
                return 0;

            var osuHitObject = (OsuHitObject)(currObj.BaseObject);

            double scalingFactor = 52.0 / osuHitObject.Radius;
            double smallDistNerf = 1.0;
            double cumulativeStrainTime = 0.0;

            double result = 0.0;

            OsuDifficultyHitObject lastLoopObj = currObj;

            double angleRepeatCount = 0.0;

            // This is iterating backwards in time from the current object.
            for (int i = 0; i < Math.Min(currObj.Index, 10); i++)
            {
                var loopObj = (OsuDifficultyHitObject)currObj.Previous(i);
                var loopHitObject = (OsuHitObject)(loopObj.BaseObject);

                cumulativeStrainTime += lastLoopObj.AdjustedDeltaTime;

                if (!(loopObj.BaseObject is Spinner))
                {
                    double jumpDistance = (osuHitObject.StackedPosition - loopHitObject.StackedEndPosition).Length;

                    // We want to nerf objects that can be easily seen within the Flashlight circle radius.
                    if (i == 0)
                        smallDistNerf = Math.Min(1.0, jumpDistance / 75.0);

                    // We also want to nerf stacks so that only the first object of the stack is accounted for.
                    double stackNerf = Math.Min(1.0, (loopObj.LazyJumpDistance / scalingFactor) / 25.0);

                    // Bonus based on how visible the object is.
                    double opacityBonus = 1.0 + max_opacity_bonus * (1.0 - currObj.OpacityAt(loopHitObject.StartTime, mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value)));

                    result += stackNerf * opacityBonus * scalingFactor * jumpDistance / cumulativeStrainTime;

                    if (loopObj.Angle != null && currObj.Angle != null)
                    {
                        // Objects further back in time should count less for the nerf.
                        if (Math.Abs(loopObj.Angle.Value - currObj.Angle.Value) < 0.02)
                            angleRepeatCount += Math.Max(1.0 - 0.1 * i, 0.0);
                    }
                }

                lastLoopObj = loopObj;
            }

            result = Math.Pow(smallDistNerf * result, 2.0);

            // Additional bonus for Hidden due to there being no approach circles.
            if (mods.OfType<OsuModHidden>().Any())
                result *= 1.0 + hidden_bonus;

            // Nerf patterns with repeated angles.
            result *= min_angle_multiplier + (1.0 - min_angle_multiplier) / (angleRepeatCount + 1.0);

            double sliderBonus = 0.0;

            if (currObj.BaseObject is Slider osuSlider)
            {
                // Invert the scaling factor to determine the true travel distance independent of circle size.
                double pixelTravelDistance = currObj.LazyTravelDistance / scalingFactor;

                // Reward sliders based on velocity.
                sliderBonus = Math.Pow(Math.Max(0.0, pixelTravelDistance / currObj.TravelTime - min_velocity), 0.5);

                // Longer sliders require more memorisation.
                sliderBonus *= pixelTravelDistance;

                // Nerf sliders with repeats, as less memorisation is required.
                if (osuSlider.RepeatCount > 0)
                    sliderBonus /= (osuSlider.RepeatCount + 1);
            }

            result += sliderBonus * slider_multiplier;

            return result;
        }
    }
}
