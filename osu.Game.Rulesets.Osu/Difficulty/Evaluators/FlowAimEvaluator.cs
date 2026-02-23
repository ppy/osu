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
            // The reason why this exist in evaluator instead of FlowAim skill - it's because it's very important to keep flowaim in the same scaling as snapaim on evaluator level
            const double flow_multiplier = 6.05;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            // Rescale the distance
            // We use the power on normalized distance so we don't have to rebalance everything when changing the exponent
            double distance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            distance = Math.Pow(distance / diameter, 1.5) * Math.Pow(diameter, 2);

            // Calculate the base difficulty by using rescaled distance and time
            double flowDifficulty = distance / Math.Pow(osuCurrObj.AdjustedDeltaTime, 2);

            double angleBonus = 0;

            if (osuCurrObj.AngleSigned != null && osuLast0Obj.AngleSigned != null && osuLast1Obj.AngleSigned != null)
            {
                double acuteAngleBonus = CalculateFlowAcuteAngleBonus(current);
                double angleChangeBonus = CalculateFlowAngleChangeBonus(current);

                // If all three notes are overlapping - don't reward angle bonuses as you don't have to do additional movement
                double overlappedNotesWeight = 1;

                if (current.Index > 2)
                {
                    double o1 = getOverlapness(osuCurrObj, osuLast0Obj);
                    double o2 = getOverlapness(osuCurrObj, osuLast1Obj);
                    double o3 = getOverlapness(osuLast0Obj, osuLast1Obj);

                    overlappedNotesWeight = 1 - o1 * o2 * o3;
                }

                angleBonus = Math.Max(acuteAngleBonus, angleChangeBonus) * overlappedNotesWeight;
            }

            // Add all bonuses
            flowDifficulty += angleBonus;
            flowDifficulty *= flow_multiplier * Math.Sqrt(osuCurrObj.SmallCircleBonus);

            // Add in additional slider velocity bonus
            // In order for compensate for lack of slider velocity in base difficulty - increase this bonus
            if (withSliderTravelDistance)
                flowDifficulty += AimEvaluator.CalculateSliderBonus(osuCurrObj) * 1.8;

            return flowDifficulty;
        }

        private static double getOverlapness(OsuDifficultyHitObject odho1, OsuDifficultyHitObject odho2)
        {
            OsuHitObject o1 = (OsuHitObject)odho1.BaseObject, o2 = (OsuHitObject)odho2.BaseObject;

            double distance = Vector2.Distance(o1.StackedPosition, o2.StackedPosition);
            double radius = o1.Radius;

            return Math.Clamp(1 - Math.Pow(Math.Max(distance - radius, 0) / radius, 2), 0, 1);
        }

        // Not used for now
        public static double CalculateSpeedflowBonus(DifficultyHitObject current)
        {
            const double base_speedflow_multiplier = 0.00; // Base multiplier for speedflow bonus
            const double bpm_factor = 18; // How steep the bonus is, higher values means more bonus for high BPM

            var osuCurrObj = (OsuDifficultyHitObject)current;

            // Autobalance, it's expected for bonus multiplier to be 1 for the bpm base
            double bpmBase = DifficultyCalculationUtils.BPMToMilliseconds(220, 4);
            double bpmFactorMultiplierAtBase = bpmBase / (bpmBase - bpm_factor) - 1;
            double multiplier = base_speedflow_multiplier / bpmFactorMultiplierAtBase;

            // Start from base of the bonus
            double speedflowBonus = multiplier * diameter / osuCurrObj.AdjustedDeltaTime;

            // Spacing factor, reward up to 1 radius. The reason why we want to buff primarily low spacing speedflow.
            // Explanation about formula: it goes fast from 0 and then slow downs, capping out on 1 radius.
            // To achieve this we use negative radius as an argument and then cut down the range to reward starting from zero distance.
            speedflowBonus *= 2 * (DifficultyCalculationUtils.Smoothstep(osuCurrObj.JumpDistance, -radius, radius) - 0.5);

            // Bpm factor
            speedflowBonus *= (osuCurrObj.AdjustedDeltaTime / (osuCurrObj.AdjustedDeltaTime - bpm_factor) - 1);

            return speedflowBonus;
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

            // Use d/t^2 velocity as a base
            double bonusBase = osuCurrObj.JumpDistance * diameter / Math.Pow(osuCurrObj.AdjustedDeltaTime, 2);

            double acuteAngleBonus = bonusBase * AimEvaluator.CalcAcuteAngleBonus(currAngle);

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
            double minVelocity = Math.Min(currVelocity, prevVelocity);

            // Adjust to d/t^2 to match evaluator scaling
            double bonusBase = minVelocity * diameter / osuCurrObj.AdjustedDeltaTime;

            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * bonusBase;

            // Take the largest of last 3 distances and if it's too small - decrease flow angle change bonus, because it's cheesable
            angleChangeBonus *= DifficultyCalculationUtils.ReverseLerp(Math.Max(osuCurrObj.JumpDistance, osuLast1Obj.JumpDistance), 0, diameter);

            return angleChangeBonus * angle_change_bonus_multiplier;
        }
    }
}
