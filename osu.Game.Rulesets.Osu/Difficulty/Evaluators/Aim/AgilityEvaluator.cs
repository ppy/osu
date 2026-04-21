// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class AgilityEvaluator
    {
        private const double distance_cap = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.2; // 1.25 circles distance between centers

        /// <summary>
        /// Evaluates the difficulty of fast aiming
        /// </summary>
        public static double EvaluateDifficultyOf(OsuDifficultyHitObject currObj)
        {
            if (currObj.BaseObject is Spinner)
                return 0;

            var prevObj = currObj.Index > 0 ? (OsuDifficultyHitObject)currObj.Previous(0) : null;

            double travelDistance = prevObj?.LazyTravelDistance ?? 0;
            double distance = travelDistance + currObj.LazyJumpDistance;

            double distanceScaled = Math.Min(distance, distance_cap) / distance_cap;

            double strain = distanceScaled * 1000 / currObj.AdjustedDeltaTime;

            strain *= Math.Pow(currObj.SmallCircleBonus, 1.5);

            strain *= highBpmBonus(currObj.AdjustedDeltaTime);

            return strain;
        }

        private static double highBpmBonus(double ms) => 1 / (1 - Math.Pow(0.2, ms / 1000));
    }
}
