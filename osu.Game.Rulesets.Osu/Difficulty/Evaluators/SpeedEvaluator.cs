// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class SpeedEvaluator
    {
        private const double min_speed_bonus = 200; // 200 BPM 1/4th
        private const double speed_balancing_factor = 40;

        /// <summary>
        /// Evaluates the difficulty of tapping the current object, based on:
        /// <list type="bullet">
        /// <item><description>time between pressing the previous and current object,</description></item>
        /// <item><description>and how easily they can be cheesed.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = current.Index > 0 ? (OsuDifficultyHitObject)current.Previous(0) : null;
            var osuNextObj = (OsuDifficultyHitObject?)osuCurrObj.Next(0);

            double strainTime = osuCurrObj.AdjustedDeltaTime;
            double doubletapness = 1.0 - osuCurrObj.GetDoubletapness(osuPrevObj, osuNextObj);

            // Cap deltatime to the OD 300 hitwindow.
            // 0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
            strainTime /= Math.Clamp((strainTime / osuCurrObj.HitWindowGreat) / 0.93, 0.92, 1);

            // speedBonus will be 0.0 for BPM < 200
            double speedBonus = 0.0;

            // Add additional scaling bonus for streams/bursts higher than 200bpm
            if (DifficultyCalculationUtils.MillisecondsToBPM(strainTime) > min_speed_bonus)
                speedBonus = 0.75 * Math.Pow((DifficultyCalculationUtils.BPMToMilliseconds(min_speed_bonus) - strainTime) / speed_balancing_factor, 2);

            // Base difficulty with all bonuses
            double difficulty = (1 + speedBonus) * 1000 / strainTime;

            difficulty *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            // Apply penalty if there's doubletappable or gallopable doubles
            return difficulty * doubletapness;
        }

        private static double highBpmBonus(double ms) => 1 / (1 - Math.Pow(0.3, ms / 1000));
    }
}
