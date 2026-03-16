// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class FlowAimEvaluator
    {
        private const double velocity_change_multiplier = 2.0;

        /// <summary>
        /// Evaluates difficulty of "flow aim" - aiming pattern where player doesn't stop their cursor on every object and instead "flows" through them.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);

            double currDistance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double prevDistance = withSliderTravelDistance ? osuLastObj.LazyJumpDistance : osuLastObj.JumpDistance;

            double currVelocity = currDistance / osuCurrObj.AdjustedDeltaTime;

            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                // If the last object is a slider, then we extend the travel velocity through the slider into the current object.
                double sliderDistance = osuLastObj.LazyTravelDistance + osuCurrObj.LazyJumpDistance;
                currVelocity = Math.Max(currVelocity, sliderDistance / osuCurrObj.AdjustedDeltaTime);
            }

            double prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;

            double flowDifficulty = currVelocity;

            // Apply high circle size bonus to the base velocity.
            // We use reduced CS bonus here because the bonus was made for an evaluator with a different d/t scaling
            flowDifficulty *= Math.Pow(osuCurrObj.SmallCircleBonus, 0.75);

            // Rhythm changes are harder to flow
            flowDifficulty *= 1 + Math.Min(0.25,
                Math.Pow((Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) - Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime)) / 50, 4));

            if (osuCurrObj.AngularVelocity != null)
            {
                // Low angular velocity flow (angles are consistent) is easier to follow than erratic flow
                flowDifficulty *= 0.8 + Math.Sqrt(osuCurrObj.AngularVelocity.Value / 270.0);
            }

            // If all three notes are overlapping - don't reward bonuses as you don't have to do additional movement
            double overlappedNotesWeight = 1;

            if (current.Index > 2)
            {
                double o1 = calculateOverlapFactor(osuCurrObj, osuLastObj);
                double o2 = calculateOverlapFactor(osuCurrObj, osuLastLastObj);
                double o3 = calculateOverlapFactor(osuLastObj, osuLastLastObj);

                overlappedNotesWeight = 1 - o1 * o2 * o3;
            }

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
            {
                // Acute angles are also hard to flow
                // We square root velocity to make acute angle switches in streams aren't having difficulty higher than snap
                flowDifficulty += Math.Sqrt(currVelocity) *
                                  SnapAimEvaluator.CalcAcuteAngleBonus(osuCurrObj.Angle.Value) *
                                  overlappedNotesWeight;
            }

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
                double overlapVelocityBuff = Math.Min(OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime),
                    Math.Abs(prevVelocity - currVelocity));

                flowDifficulty += overlapVelocityBuff * distRatio * velocity_change_multiplier;
            }

            if (osuCurrObj.BaseObject is Slider)
            {
                // Include slider velocity to make velocity more consistent with snap
                flowDifficulty += osuCurrObj.TravelDistance / osuCurrObj.TravelTime;
            }

            // Final velocity is being raised to a power because flow difficulty scales harder with both high distance and time, and we want to account for that
            return Math.Pow(flowDifficulty, 1.45);
        }

        private static double calculateOverlapFactor(OsuDifficultyHitObject first, OsuDifficultyHitObject second)
        {
            var firstBase = (OsuHitObject)first.BaseObject;
            var secondBase = (OsuHitObject)second.BaseObject;
            double objectRadius = firstBase.Radius;

            double distance = Vector2.Distance(firstBase.StackedPosition, secondBase.StackedPosition);
            return Math.Clamp(1 - Math.Pow(Math.Max(distance - objectRadius, 0) / objectRadius, 2), 0, 1);
        }
    }
}
