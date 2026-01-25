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
        // The reason why this exist in evaluator instead of FlowAim skill - it's because it's very important to keep flowaim in the same scaling as snapaim on evaluator level
        private const double flow_multiplier = 6.2;

        private const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
        private const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            // Start with velocity
            double velocity = osuCurrObj.LazyJumpDistance / osuCurrObj.AdjustedDeltaTime;

            if (osuLast0Obj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLast0Obj.TravelDistance / osuLast0Obj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                velocity = Math.Max(velocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            double distancePowerAddition;

            if (osuCurrObj.LazyJumpDistance > diameter)
            {
                // Controls distance scaling for high spaced flow aim
                distancePowerAddition = Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.45);
            }
            else
            {
                // Controls distance scaling for low spaced flow aim
                distancePowerAddition = Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.7);
            }

            // Rescale velocity by raising t to the power of 2 and distance to the power determined earlier
            double flowDifficulty = velocity * (distancePowerAddition * diameter) / osuCurrObj.AdjustedDeltaTime;

            // Flow aim is harder on High BPM
            const double base_speedflow_multiplier = 0.1; // Base multiplier for speedflow bonus
            const double bpm_factor = 18; // How steep the bonus is, higher values means more bonus for high BPM

            // Autobalance, it's expected for bonus multiplier to be 1 for the bpm base
            double bpmBase = DifficultyCalculationUtils.BPMToMilliseconds(220, 4);
            double bpmFactorMultiplierAtBase = bpmBase / (bpmBase - bpm_factor) - 1;
            double multiplier = base_speedflow_multiplier / bpmFactorMultiplierAtBase;




            double angleBonus = 0;

            if (osuCurrObj.AngleSigned != null && osuLast0Obj.AngleSigned != null && osuLast1Obj.AngleSigned != null)
            {
                double acuteAngleBonus = CalculateFlowAcuteAngleBonus(current);
                double angleChangeBonus = CalculateFlowAngleChangeBonus(current);

                // Don't account for distance bonuse here
                angleBonus = Math.Max(acuteAngleBonus, angleChangeBonus) / Math.Max(distancePowerAddition, 0.01);

                // If all three notes are overlapping - don't reward angle bonuses as you don't have to do additional movement
                double overlappedNotesWeight = 1;

                if (current.Index > 2)
                {
                    double o1 = getOverlapness(osuCurrObj, osuLast0Obj);
                    double o2 = getOverlapness(osuCurrObj, osuLast1Obj);
                    double o3 = getOverlapness(osuLast0Obj, osuLast1Obj);

                    overlappedNotesWeight = 1 - o1 * o2 * o3;
                }

                angleBonus *= overlappedNotesWeight;
            }

            double speedflowBonus = CalculateSpeedflowBonus(current);

            // Add all bonuses
            flowDifficulty *= (1 + angleBonus);
            flowDifficulty += speedflowBonus;
            flowDifficulty *= flow_multiplier * Math.Sqrt(osuCurrObj.SmallCircleBonus);

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance)
                flowDifficulty += AimEvaluator.CalculateSliderBonus(osuCurrObj);

            return flowDifficulty;
        }

        private static double getOverlapness(OsuDifficultyHitObject odho1, OsuDifficultyHitObject odho2)
        {
            OsuHitObject o1 = (OsuHitObject)odho1.BaseObject, o2 = (OsuHitObject)odho2.BaseObject;

            double distance = Vector2.Distance(o1.StackedPosition, o2.StackedPosition);
            double radius = o1.Radius;

            return Math.Clamp(1 - Math.Pow(Math.Max(distance - radius, 0) / radius, 2), 0, 1);
        }

        public static double CalculateSpeedflowBonus(DifficultyHitObject current)
        {
            const double base_speedflow_multiplier = 0.1; // Base multiplier for speedflow bonus
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
            speedflowBonus *= 2 * (DifficultyCalculationUtils.Smoothstep(osuCurrObj.LazyJumpDistance, -radius, radius) - 0.5);

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

            double acuteAngleBonus = AimEvaluator.CalcAcuteAngleBonus(currAngle);

            // If spacing is too low - decrease reward
            acuteAngleBonus *= DifficultyCalculationUtils.ReverseLerp(osuCurrObj.LazyJumpDistance, radius, diameter);

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
            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.AdjustedDeltaTime;
            double prevVelocity = osuLast0Obj.LazyJumpDistance / osuLast0Obj.AdjustedDeltaTime;
            double minVelocity = Math.Min(currVelocity, prevVelocity);
            double bonusBase = minVelocity / Math.Max(currVelocity, 0.01);

            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * bonusBase;

            // Take the largest of last 3 distances and if it's too small - decrease flow angle change bonus, because it's cheesable
            angleChangeBonus *= DifficultyCalculationUtils.ReverseLerp(Math.Max(osuCurrObj.LazyJumpDistance, osuLast1Obj.LazyJumpDistance), 0, diameter);

            return angleChangeBonus * angle_change_bonus_multiplier;
        }
    }
}
