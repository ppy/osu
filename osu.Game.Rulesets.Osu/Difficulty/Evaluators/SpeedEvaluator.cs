// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class SpeedEvaluator
    {
        private const double single_spacing_threshold = 125;
        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        /// <summary>
        /// Evaluates the difficulty of tapping the current object, based on:
        /// <list type="bullet">
        /// <item><description>time between pressing the previous and current object,</description></item>
        /// <item><description>distance between those objects,</description></item>
        /// <item><description>and how easily they can be cheesed.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            // derive strainTime for calculation
            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = current.Index > 0 ? (OsuDifficultyHitObject)current.Previous(0) : null;
            var osuNextObj = (OsuDifficultyHitObject?)current.Next(0);

            double strainTime = osuCurrObj.StrainTime;
            double doubletapness = 1;

            // Nerf doubletappable doubles.
            if (osuNextObj != null)
            {
                double currDeltaTime = Math.Max(1, osuCurrObj.DeltaTime);
                double nextDeltaTime = Math.Max(1, osuNextObj.DeltaTime);
                double deltaDifference = Math.Abs(nextDeltaTime - currDeltaTime);
                double speedRatio = currDeltaTime / Math.Max(currDeltaTime, deltaDifference);
                double windowRatio = Math.Pow(Math.Min(1, currDeltaTime / osuCurrObj.HitWindowGreat), 2);
                doubletapness = Math.Pow(speedRatio, 1 - windowRatio);
            }

            // Cap deltatime to the OD 300 hitwindow.
            // 0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
            strainTime /= Math.Clamp((strainTime / osuCurrObj.HitWindowGreat) / 0.93, 0.92, 1);

            // derive speedBonus for calculation
            double speedBonus = 1.0;

            if (strainTime < min_speed_bonus)
                speedBonus = 1 + 0.75 * Math.Pow((min_speed_bonus - strainTime) / speed_balancing_factor, 2);

            double distance = Math.Min(single_spacing_threshold, osuCurrObj.JumpDistance);

            double sliderStreamMultiplier = 1;

            if (osuCurrObj.BaseObject is Slider slider && osuPrevObj?.BaseObject is Slider)
            {
                // Bonus base
                double sliderStreamBonus = 0.25;

                // If slider was slower than notes before - punish it
                if (osuCurrObj.StrainTime > osuPrevObj.StrainTime)
                    sliderStreamBonus *= Math.Pow(Math.Min(osuCurrObj.StrainTime, osuPrevObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuPrevObj.StrainTime), 2);

                // Punish too short sliders to prevent cheesing (cheesing is still possible, but it's very rare)
                double sliderLength = 2 * slider.Velocity * slider.SpanDuration;
                if (sliderLength < slider.Radius)
                    sliderStreamBonus *= sliderLength / slider.Radius;

                sliderStreamMultiplier += sliderStreamBonus;
            }

            return (speedBonus + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) * doubletapness * sliderStreamMultiplier / strainTime;
        }
    }
}
