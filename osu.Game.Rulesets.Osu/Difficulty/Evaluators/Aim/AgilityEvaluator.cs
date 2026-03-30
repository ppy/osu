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
        private const double wide_angle_multiplier = 0.6;

        /// <summary>
        /// Evaluates the difficulty of fast aiming
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = current.Index > 0 ? (OsuDifficultyHitObject)current.Previous(0) : null;
            var osuPrevObj1 = current.Index > 1 ? (OsuDifficultyHitObject)current.Previous(1) : null;

            double strain = getStrain(osuCurrObj, osuPrevObj);

            if (osuCurrObj.Angle != null && osuPrevObj != null)
            {
                double wideAngleBonus = SnapAimEvaluator.CalcWideAngleBonus(osuCurrObj.Angle.Value);
                wideAngleBonus *= DifficultyCalculationUtils.ReverseLerp(osuPrevObj.AdjustedDeltaTime, osuCurrObj.AdjustedDeltaTime * 0.5, osuCurrObj.AdjustedDeltaTime * 0.75);

                double strainPrev = getStrain(osuPrevObj, osuPrevObj1);
                strain += Math.Min(strain, strainPrev) * wideAngleBonus * wide_angle_multiplier;
            }

            strain *= Math.Pow(osuCurrObj.SmallCircleBonus, 1.5);
            strain *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            return strain * DifficultyCalculationUtils.Smootherstep(osuCurrObj.LazyJumpDistance, 0, OsuDifficultyHitObject.NORMALISED_RADIUS);
        }

        private static double getStrain(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject? osuPrevObj)
        {
            double travelDistance = osuPrevObj?.LazyTravelDistance ?? 0;
            double distance = travelDistance + osuCurrObj.LazyJumpDistance;
            double distanceScaled = Math.Min(distance, distance_cap) / distance_cap;
            return distanceScaled * 1000 / osuCurrObj.AdjustedDeltaTime;
        }

        private static double highBpmBonus(double ms) => 1 / (1 - Math.Pow(0.2, ms / 1000));
    }
}
