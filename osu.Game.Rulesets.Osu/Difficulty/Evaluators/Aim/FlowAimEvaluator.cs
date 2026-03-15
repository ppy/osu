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
        private const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
        private const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

        /// <summary>
        /// Evaluates difficulty of "flow aim" - aiming pattern where player doesn't stop their cursor on every object and instead "flows" through them.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            const double flow_multiplier = 0.95;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            double distance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double flowDifficulty = distance / osuCurrObj.AdjustedDeltaTime;

            if (osuCurrObj.AngularVelocity != null)
            {
                // Low angular velocity flow (angles are consistent) is easier to follow than erratic flow
                flowDifficulty *= 0.8 + Math.Sqrt(osuCurrObj.AngularVelocity.Value / 270.0);
            }

            double angleBonus = 0;

            if (osuCurrObj.AngleSigned != null && osuLast0Obj.AngleSigned != null && osuLast1Obj.AngleSigned != null)
            {
                double acuteAngleBonus = CalculateFlowAcuteAngleBonus(current);
                double angleChangeBonus = CalculateFlowAngleChangeBonus(current);

                // If all three notes are overlapping - don't reward angle bonuses as you don't have to do additional movement
                double overlappedNotesWeight = 1;

                if (current.Index > 2)
                {
                    double o1 = calculateOverlapFactor(osuCurrObj, osuLast0Obj);
                    double o2 = calculateOverlapFactor(osuCurrObj, osuLast1Obj);
                    double o3 = calculateOverlapFactor(osuLast0Obj, osuLast1Obj);

                    overlappedNotesWeight = 1 - o1 * o2 * o3;
                }

                angleBonus = Math.Max(acuteAngleBonus, angleChangeBonus) * overlappedNotesWeight;
            }

            // Add all bonuses
            flowDifficulty += angleBonus;

            // Apply high circle size bonus to the base velocity
            flowDifficulty *= osuCurrObj.SmallCircleBonus;

            flowDifficulty *= flow_multiplier;

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

        // This bonus accounts for the fact that flow is circular movement, therefore flowing on sharp angles is harder.
        public static double CalculateFlowAcuteAngleBonus(DifficultyHitObject current)
        {
            const double acute_angle_bonus_multiplier = 1.05;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;

            if (osuCurrObj.Angle == null)
                return 0;

            double currAngle = (double)osuCurrObj.Angle;

            double bonusBase = osuCurrObj.JumpDistance / osuCurrObj.AdjustedDeltaTime;

            double acuteAngleBonus = bonusBase * SnapAimEvaluator.CalcAcuteAngleBonus(currAngle);

            // If spacing is too low - decrease reward
            acuteAngleBonus *= DifficultyCalculationUtils.ReverseLerp(osuCurrObj.JumpDistance, radius, diameter);

            return acuteAngleBonus * acute_angle_bonus_multiplier;
        }

        // This bonus accounts for flow aim being harder when angle is changing.
        public static double CalculateFlowAngleChangeBonus(DifficultyHitObject current)
        {
            const double angle_change_bonus_multiplier = 1.0;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            if (osuCurrObj.AngleSigned == null || osuLast0Obj.AngleSigned == null)
                return 0;

            double currAngle = osuCurrObj.AngleSigned.Value;
            double lastAngle = osuLast0Obj.AngleSigned.Value;

            // Take min velocity to avoid abuse with very small spacing
            double currVelocity = osuCurrObj.JumpDistance / osuCurrObj.AdjustedDeltaTime;
            double prevVelocity = osuLast0Obj.JumpDistance / osuLast0Obj.AdjustedDeltaTime;

            double bonusBase = Math.Min(currVelocity, prevVelocity);

            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * bonusBase;

            // Take the largest of last 3 distances and if it's too small - decrease flow angle change bonus, because it's cheesable
            angleChangeBonus *= DifficultyCalculationUtils.ReverseLerp(Math.Max(osuCurrObj.JumpDistance, osuLast1Obj.JumpDistance), radius, diameter);

            return angleChangeBonus * angle_change_bonus_multiplier;
        }
    }
}
