// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.ObjectExtensions;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class SpeedEvaluator
    {
        private const double single_spacing_threshold = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25; // 1.25 circles distance between centers
        private const double min_speed_bonus = 200; // 200 BPM 1/4th
        private const double speed_balancing_factor = 40;
        private const double distance_multiplier = 0.9;

        /// <summary>
        /// Evaluates the difficulty of tapping the current object, based on:
        /// <list type="bullet">
        /// <item><description>time between pressing the previous and current object,</description></item>
        /// <item><description>distance between those objects,</description></item>
        /// <item><description>and how easily they can be cheesed.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, IReadOnlyList<Mod> mods)
        {
            if (current.BaseObject is Spinner)
                return 0;

            // derive strainTime for calculation
            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = current.Index > 0 ? (OsuDifficultyHitObject)current.Previous(0) : null;
            var osuNextObj = (OsuDifficultyHitObject?)osuCurrObj.Next(0);

            double strainTime = osuCurrObj.StrainTime;

            // Nerf doubletappable doubles.
            double currDeltaTime = Math.Max(1, osuCurrObj.DeltaTime);

            // It's easier to gallop if you have more time between doubles
            // Get max between next and prev ratio to avoid nerfing triples
            double speedRatio = Math.Max(getSpeedRatio(osuCurrObj, osuPrevObj), getSpeedRatio(osuCurrObj, osuNextObj));

            // Can't doubletap if circles don't intersect
            double normalizedDistance = Math.Min(1, osuCurrObj.LazyJumpDistance / (OsuDifficultyHitObject.NORMALISED_RADIUS * 2));
            double distanceFactor = normalizedDistance < 0.5 ? 1.0 : 1 - Math.Pow((normalizedDistance - 0.5) / 0.5, 0.5);

            // Use HitWindowGreat * 2, because even if you can't get 300 with doubletapping - you still can gallop
            const double power = 2;
            double windowRatio = Math.Pow(Math.Min(1, currDeltaTime / (osuCurrObj.HitWindowGreat * 2)), power);

            // Nerf even more if you don't need to gallop anymore
            double halfPoint = Math.Pow(0.5, power);
            if (windowRatio < halfPoint)
                windowRatio *= windowRatio / halfPoint;

            double doubletapness = Math.Pow(speedRatio, distanceFactor * (1 - windowRatio));

            // Cap deltatime to the OD 300 hitwindow.
            // 0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
            strainTime /= Math.Clamp((strainTime / osuCurrObj.HitWindowGreat) / 0.93, 0.92, 1);

            // speedBonus will be 0.0 for BPM < 200
            double speedBonus = 0.0;

            // Add additional scaling bonus for streams/bursts higher than 200bpm
            if (DifficultyCalculationUtils.MillisecondsToBPM(strainTime) > min_speed_bonus)
                speedBonus = 0.75 * Math.Pow((DifficultyCalculationUtils.BPMToMilliseconds(min_speed_bonus) - strainTime) / speed_balancing_factor, 2);

            double travelDistance = osuPrevObj?.TravelDistance ?? 0;
            double distance = travelDistance + osuCurrObj.MinimumJumpDistance;

            // Cap distance at single_spacing_threshold
            distance = Math.Min(distance, single_spacing_threshold);

            // Max distance bonus is 1 * `distance_multiplier` at single_spacing_threshold
            double distanceBonus = Math.Pow(distance / single_spacing_threshold, 3.95) * distance_multiplier;

            if (mods.OfType<OsuModAutopilot>().Any())
                distanceBonus = 0;

            // Base difficulty with all bonuses
            double difficulty = (1 + speedBonus + distanceBonus) * 1000 / strainTime;

            // Apply penalty if there's doubletappable doubles
            return difficulty * doubletapness;
        }

        private static double getSpeedRatio(OsuDifficultyHitObject current, OsuDifficultyHitObject? other)
        {
            if (other.IsNull())
                return 0;

            double currDeltaTime = Math.Max(1, current.DeltaTime);
            double otherDeltaTime = Math.Max(1, other.DeltaTime);

            double deltaDifference = Math.Abs(currDeltaTime - otherDeltaTime);

            return currDeltaTime / Math.Max(currDeltaTime, deltaDifference);
        }
    }
}
