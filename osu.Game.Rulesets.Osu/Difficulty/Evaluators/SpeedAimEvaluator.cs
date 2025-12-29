// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class SpeedAimEvaluator
    {
        private const double single_spacing_threshold = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25; // 1.25 circles distance between centers

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

            double strainTime = osuCurrObj.AdjustedDeltaTime;

            double travelDistance = osuPrevObj?.TravelDistance ?? 0;
            double distance = travelDistance + osuCurrObj.MinimumJumpDistance;

            // Cap distance at single_spacing_threshold
            distance = Math.Min(distance, single_spacing_threshold);

            // Max distance bonus is 1 * `distance_multiplier` at single_spacing_threshold
            double distanceBonus = Math.Pow(distance / single_spacing_threshold, 3.95);

            // Apply reduced small circle bonus because flow aim difficulty on small circles doesn't scale as hard as jumps
            distanceBonus *= Math.Sqrt(osuCurrObj.SmallCircleBonus);

            if (mods.OfType<OsuModAutopilot>().Any())
                distanceBonus = 0;

            return distanceBonus * 1000 / strainTime;
        }
    }
}
