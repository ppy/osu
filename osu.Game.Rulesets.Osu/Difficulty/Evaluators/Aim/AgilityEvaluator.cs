// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class AgilityEvaluator
    {
        /// <summary>
        /// Evaluates the difficulty of fast aiming
        /// </summary>
        public static double EvaluateDifficultyOf(OsuDifficultyHitObject currObj)
        {
            if (currObj.BaseObject is Spinner)
                return 0;

            const double distance_cap = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.2; // 1.2 circles distance between centers

            var prevObj = currObj.Index > 0 ? (OsuDifficultyHitObject)currObj.Previous(0) : null;

            double travelDistance = prevObj?.LazyTravelDistance ?? 0;
            double distance = travelDistance + currObj.LazyJumpDistance;

            double distanceScaled = Math.Min(distance, distance_cap) / distance_cap;

            double agilityDifficulty = distanceScaled * 1000 / currObj.AdjustedDeltaTime;

            agilityDifficulty *= DiffUtils.Pow(currObj.SmallCircleBonus, 1.5);

            agilityDifficulty *= highBpmBonus(currObj.AdjustedDeltaTime);

            return agilityDifficulty;
        }

        private static double highBpmBonus(double ms) => 1 / (1 - DiffUtils.Pow(0.2, ms / 1000));
    }
}
