// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class FlowAimEvaluator
    {
        private const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
        private const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            const double velocity_change_multiplier = 2.0;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            double currDistance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderDistance = osuLastObj.LazyTravelDistance + osuCurrObj.LazyJumpDistance;
                currVelocity = Math.Max(currVelocity, sliderDistance / osuCurrObj.AdjustedDeltaTime);
            }

            // As above, do the same for the previous hitobject.
            double prevDistance = withSliderTravelDistance ? osuLastObj.LazyJumpDistance : osuLastObj.JumpDistance;
            double prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;

            double flowDifficulty = currVelocity; // Start strain with regular velocity.

            // rhythm change bonus
            flowDifficulty *= 1 + Math.Min(0.25, Math.Pow((Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) - Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime)) / 50, 4));

            if (osuCurrObj.AngularVelocity != null)
            {
                // angular velocity adjustment
                flowDifficulty *= 0.8 + Math.Sqrt(osuCurrObj.AngularVelocity.Value / 270.0);
            }

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
            {
                // acute difficulty increase
                flowDifficulty += Math.Sqrt(currVelocity) * AimEvaluator.CalcAcuteAngleBonus(osuCurrObj.Angle.Value);
            }

            double velocityChangeBonus = 0;

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                if (withSliderTravelDistance)
                {
                    currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;
                    prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;
                }

                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = DifficultyCalculationUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                velocityChangeBonus *= Math.Pow(Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) / Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), 2);
            }

            flowDifficulty += velocityChangeBonus * velocity_change_multiplier;

            // Apply high circle size bonus
            flowDifficulty *= Math.Pow(osuCurrObj.SmallCircleBonus, 2);

            return Math.Pow(flowDifficulty, 1.45);
        }

        private static double highBpmBonus(double ms) => 1 / (1 - Math.Pow(0.15, Math.Pow(ms / 1000, 0.65)));
    }
}
