// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class AgilityEvaluator
    {
        private const double distance_cap = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25; // 1.25 circles distance between centers

        /// <summary>
        /// Evaluates the difficulty of fast aiming
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = current.Index > 0 ? (OsuDifficultyHitObject)current.Previous(0) : null;

            double travelDistance = osuPrevObj?.LazyTravelDistance ?? 0;
            double distance = travelDistance + osuCurrObj.LazyJumpDistance;

            double distanceScaled = Math.Min(distance, distance_cap) / distance_cap;

            double strain = distanceScaled * 1000 / osuCurrObj.AdjustedDeltaTime;

            strain *= Math.Pow(osuCurrObj.SmallCircleBonus, 1.5);

            strain *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            return strain * DifficultyCalculationUtils.Smootherstep(distance, 0, OsuDifficultyHitObject.NORMALISED_RADIUS);
        }

        private static double highBpmBonus(double ms) => 1 / (1 - Math.Pow(0.2, ms / 1000));
    }
}
