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
        /// <summary>
        /// Evaluates the difficulty of fast aiming
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            const double previous_delta_influence = 0.5;

            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = (OsuDifficultyHitObject?)current.Previous();

            // Assume that the player always uses at least the full great hitwindow for the movement
            double currentDelta = Math.Max(osuCurrObj.AdjustedDeltaTime, osuCurrObj.HitWindowGreat);

            // For objects that are stacked we want to reduce the agility difficulty slightly by combining delta times of both objects together
            // Because we can assume that they likely would be done in one movement.
            double previousDelta = 0;

            if (osuPrevObj != null)
            {
                previousDelta = osuPrevObj.AdjustedDeltaTime *
                                DiffUtils.ReverseLerp(osuPrevObj.LazyJumpDistance, OsuDifficultyHitObject.NORMALISED_RADIUS, 0);
            }

            double combinedDelta = currentDelta + previousDelta * previous_delta_influence;

            double angleSwitchingBonus = calculateWideAngleBonus(osuCurrObj, osuPrevObj);

            double agilityDifficulty = (1 + angleSwitchingBonus) * 10_000_000 / DiffUtils.Pow(combinedDelta, 3.1);

            agilityDifficulty *= osuCurrObj.SmallCircleBonus;

            return agilityDifficulty;
        }

        private static double calculateWideAngleBonus(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject? osuPrevObj)
        {
            const double wide_bonus_multiplier = 2.5;

            if (osuCurrObj.Angle == null || osuPrevObj?.Angle == null)
                return 0;

            double wideness = DiffUtils.Smootherstep(osuCurrObj.Angle.Value, double.DegreesToRadians(0), double.DegreesToRadians(180));

            double rhythmDifferencePenalty = DiffUtils.Pow(Math.Min(osuCurrObj.AdjustedDeltaTime, osuPrevObj.AdjustedDeltaTime) / Math.Max(osuCurrObj.AdjustedDeltaTime, osuPrevObj.AdjustedDeltaTime),
                3);

            return wideness *
                   rhythmDifferencePenalty *
                   DiffUtils.ReverseLerp(osuPrevObj.LazyJumpDistance, OsuDifficultyHitObject.NORMALISED_RADIUS, OsuDifficultyHitObject.NORMALISED_DIAMETER) *
                   wide_bonus_multiplier;
        }
    }
}
