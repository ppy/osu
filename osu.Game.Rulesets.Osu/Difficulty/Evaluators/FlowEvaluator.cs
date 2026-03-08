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

        public static double LiveEvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            const double acute_angle_multiplier = 2.5;
            const double slider_multiplier = 1.9;
            const double velocity_change_multiplier = 1.1;
            const double wiggle_multiplier = 1.02; // WARNING: Increasing this multiplier beyond 1.02 reduces difficulty as distance increases. Refer to the desmos link above the wiggle bonus calculation

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);

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

            double acuteAngleBonus = 0;
            double sliderBonus = 0;
            double velocityChangeBonus = 0;
            double wiggleBonus = 0;

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
            {
                double currAngle = osuCurrObj.Angle.Value;
                double lastAngle = osuLastObj.Angle.Value;

                // Rewarding angles, take the smaller velocity as base.
                double angleBonus = Math.Min(currVelocity, prevVelocity);

                acuteAngleBonus = angleBonus * AimEvaluator.CalcAcuteAngleBonus(currAngle);

                // Apply wiggle bonus for jumps that are [radius, 3*diameter] in distance, with < 110 angle
                // https://www.desmos.com/calculator/dp0v0nvowc
                wiggleBonus = angleBonus
                              * DifficultyCalculationUtils.Smootherstep(currDistance, radius, diameter)
                              * Math.Pow(DifficultyCalculationUtils.ReverseLerp(currDistance, diameter * 3, diameter), 1.8)
                              * DifficultyCalculationUtils.Smootherstep(currAngle, double.DegreesToRadians(110), double.DegreesToRadians(60))
                              * DifficultyCalculationUtils.Smootherstep(prevDistance, radius, diameter)
                              * Math.Pow(DifficultyCalculationUtils.ReverseLerp(prevDistance, diameter * 3, diameter), 1.8)
                              * DifficultyCalculationUtils.Smootherstep(lastAngle, double.DegreesToRadians(110), double.DegreesToRadians(60));
            }

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                if (withSliderTravelDistance)
                {
                    // We want to use the average velocity over the whole object when awarding differences, not the individual jump and slider path velocities.
                    prevVelocity = (osuLastObj.LazyJumpDistance + osuLastLastObj.TravelDistance) / osuLastObj.AdjustedDeltaTime;
                    currVelocity = (osuCurrObj.LazyJumpDistance + osuLastObj.TravelDistance) / osuCurrObj.AdjustedDeltaTime;
                }

                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = DifficultyCalculationUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                velocityChangeBonus *= Math.Pow(Math.Min(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime) / Math.Max(osuCurrObj.AdjustedDeltaTime, osuLastObj.AdjustedDeltaTime), 2);
            }

            if (osuCurrObj.BaseObject is Slider)
            {
                // Reward sliders based on velocity.
                sliderBonus = osuCurrObj.TravelDistance / osuCurrObj.TravelTime;
            }

            flowDifficulty += wiggleBonus * wiggle_multiplier;
            flowDifficulty += velocityChangeBonus * velocity_change_multiplier;

            flowDifficulty += acuteAngleBonus * acute_angle_multiplier;

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance)
                flowDifficulty += (sliderBonus < 1 ? sliderBonus : Math.Pow(sliderBonus, 0.75)) * slider_multiplier;

            // Apply high circle size bonus
            flowDifficulty *= Math.Pow(osuCurrObj.SmallCircleBonus, 1.5);

            //flowDifficulty *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            return Math.Pow(flowDifficulty, 1.5);
        }

        public static double GivyEvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            // The reason why this exist in evaluator instead of FlowAim skill - it's because it's very important to keep flowaim in the same scaling as snapaim on evaluator level
            double distance_exponent = 1.2;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            // Rescale the distance
            // We use the power on normalized distance so we don't have to rebalance everything when changing the exponent
            double distance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            //distance = Math.Pow(distance, distance_exponent);

            // Calculate the base difficulty by using rescaled distance and time
            double flowDifficulty = distance / osuCurrObj.AdjustedDeltaTime;

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

            flowDifficulty *= 1 + Math.Min(0.25, Math.Pow((Math.Max(osuCurrObj.AdjustedDeltaTime, osuLast0Obj.AdjustedDeltaTime) - Math.Min(osuCurrObj.AdjustedDeltaTime, osuLast0Obj.AdjustedDeltaTime)) / 50, 4));

            // Add all bonuses
            flowDifficulty += angleBonus;
            flowDifficulty *= Math.Pow(osuCurrObj.SmallCircleBonus, 1.5);

            // Add in additional slider velocity bonus
            // In order for compensate for lack of slider velocity in base difficulty - increase this bonus
            //if (withSliderTravelDistance)
            //    flowDifficulty += AimEvaluator.CalculateSliderBonus(osuCurrObj) * 1.8;

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance && osuCurrObj.BaseObject is Slider)
            {
                double sliderBonus = osuCurrObj.TravelDistance / osuCurrObj.TravelTime;
                flowDifficulty += (sliderBonus < 1 ? sliderBonus : Math.Pow(sliderBonus, 0.75)) * 2.5;
            }

            if (osuCurrObj.AngularVelocity != null)
            {
                flowDifficulty *= 0.8 + Math.Sqrt(osuCurrObj.AngularVelocity.Value / 270.0);
            }

            //flowDifficulty *= highBpmBonus(osuCurrObj.AdjustedDeltaTime);

            return flowDifficulty * 10;
        }

        private static double highBpmBonus(double ms) => 1 / (1 - Math.Pow(0.15, Math.Pow(ms / 1000, 0.65)));

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
            const double acute_angle_bonus_multiplier = 1.05;

            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;

            if (osuCurrObj.Angle == null)
                return 0;

            double currAngle = (double)osuCurrObj.Angle;

            double bonusBase = osuCurrObj.JumpDistance / osuCurrObj.AdjustedDeltaTime;

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
            double bonusBase = minVelocity / osuCurrObj.AdjustedDeltaTime;

            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * bonusBase;

            // Take the largest of last 3 distances and if it's too small - decrease flow angle change bonus, because it's cheesable
            angleChangeBonus *= DifficultyCalculationUtils.ReverseLerp(Math.Max(osuCurrObj.JumpDistance, osuLast1Obj.JumpDistance), 0, diameter);

            return angleChangeBonus * angle_change_bonus_multiplier;
        }
    }
}
