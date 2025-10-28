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
        private const double flow_multiplier = 1.095;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Start with velocity
            double velocity = osuCurrObj.LazyJumpDistance / osuCurrObj.AdjustedDeltaTime;

            if (osuLast0Obj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLast0Obj.TravelDistance / osuLast0Obj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                velocity = Math.Max(velocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            double flowDifficulty = velocity;

            // Rescale the distance to make it closer d/t
            if (osuCurrObj.LazyJumpDistance > diameter)
            {
                // Controls distance scaling for high spaced flow aim
                flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.4);
            }
            else
            {
                // Controls distance scaling for low spaced flow aim
                flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.8);
            }

            // Flow aim is harder on High BPM
            const double base_speedflow_multiplier = 0.07; // Base multiplier for speedflow bonus
            const double bpm_factor = 10; // How steep the bonus is, higher values means more bonus for high BPM

            // Autobalance, it's expected for bonus multiplier to be 1 for the bpm base
            double bpmBase = DifficultyCalculationUtils.BPMToMilliseconds(220, 4);
            double bpmFactorMultiplierAtBase = bpmBase / (bpmBase - bpm_factor) - 1;
            double multiplier = base_speedflow_multiplier / bpmFactorMultiplierAtBase;

            // Start from base of the bonus
            double speeflowBonus = multiplier * diameter / osuCurrObj.AdjustedDeltaTime;

            // Spacing factor, reward up to 1 radius. The reason why we're doing this is because we want to be close live speedflow
            // If we won't do this - it will be similar to multiplicative speed and distance bonuses, not additive
            speeflowBonus *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.LazyJumpDistance, -radius, radius);

            // Bpm factor
            speeflowBonus *= (osuCurrObj.AdjustedDeltaTime / (osuCurrObj.AdjustedDeltaTime - bpm_factor) - 1);

            flowDifficulty += speeflowBonus;

            double angleBonus = 0;

            if (osuCurrObj.AngleSigned != null && osuLast0Obj.AngleSigned != null && osuLast1Obj.AngleSigned != null)
            {
                double acuteAngleBonus = CalculateFlowAcuteAngleBonus(current);
                double angleChangeBonus = CalculateFlowAngleChangeBonus(current);

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

            flowDifficulty += angleBonus;

            flowDifficulty *= flow_multiplier;

            if (osuLast0Obj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderBonus = osuLast0Obj.TravelDistance / osuLast0Obj.TravelTime;
                flowDifficulty += sliderBonus * AimEvaluator.SLIDER_MULTIPLIER;
            }

            return flowDifficulty * osuCurrObj.SmallCircleBonus;
        }

        private static double getOverlapness(OsuDifficultyHitObject odho1, OsuDifficultyHitObject odho2)
        {
            OsuHitObject o1 = (OsuHitObject)odho1.BaseObject, o2 = (OsuHitObject)odho2.BaseObject;

            double distance = Vector2.Distance(o1.StackedPosition, o2.StackedPosition);
            double radius = o1.Radius;

            return Math.Clamp(1 - Math.Pow(Math.Max(distance - radius, 0) / radius, 2), 0, 1);
        }

        // This bonus accounts for the fact that flow is circular movement, therefore flowing on sharp angles is harder.
        public static double CalculateFlowAcuteAngleBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;

            if (osuCurrObj.Angle == null)
                return 0;

            double currAngle = (double)osuCurrObj.Angle;

            double currAngleBonus = AimEvaluator.CalcAcuteAngleBonus(currAngle);

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.AdjustedDeltaTime;
            double acuteAngleBonus = currVelocity * currAngleBonus;

            return acuteAngleBonus;
        }

        // This bonus accounts for flow aim being harder when angle is changing.
        public static double CalculateFlowAngleChangeBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);

            if (osuCurrObj.AngleSigned == null || osuLast0Obj.AngleSigned == null)
                return 0;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.AdjustedDeltaTime;
            double prevVelocity = osuLast0Obj.LazyJumpDistance / osuLast0Obj.AdjustedDeltaTime;

            double currAngle = osuCurrObj.AngleSigned.Value;
            double lastAngle = osuLast0Obj.AngleSigned.Value;

            double baseVelocity = Math.Min(currVelocity, prevVelocity);
            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * baseVelocity;

            return angleChangeBonus;
        }
    }
}
