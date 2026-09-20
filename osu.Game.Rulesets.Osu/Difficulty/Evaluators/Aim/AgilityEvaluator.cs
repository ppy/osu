// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            const double previous_delta_influence = 0.75;

            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = (OsuDifficultyHitObject?)current.Previous();

            // For objects that are stacked we want to reduce the agility difficulty slightly by combining delta times of both objects since we can assume that they would be done in one movement.
            double previousDelta = osuPrevObj?.AdjustedDeltaTime * DiffUtils.ReverseLerp(osuPrevObj?.LazyJumpDistance ?? 0, OsuDifficultyHitObject.NORMALISED_RADIUS, 0) ?? 0;
            double combinedDelta = osuCurrObj.AdjustedDeltaTime + previousDelta * previous_delta_influence;

            double agilityDifficulty = DiffUtils.Pow(1000 / combinedDelta, 2);

            agilityDifficulty *= DiffUtils.Pow(osuCurrObj.SmallCircleBonus, 1.5);

            return agilityDifficulty;
        }
    }
}
