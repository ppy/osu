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
        private static double flowMultiplier => 1.01;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Start with velocity
            double flowDifficulty = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;

            if (osuLast0Obj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLast0Obj.TravelDistance / osuLast0Obj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                flowDifficulty = Math.Max(flowDifficulty, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            // Square the distance to turn into d/t
            if (osuCurrObj.LazyJumpDistance > diameter)
            {
                double comfyness = IdentifyComfyCircluarFlow(current);
                flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 1 - comfyness);
            }
            else
            {
                flowDifficulty *= osuCurrObj.LazyJumpDistance / diameter;
            }

            // Flow aim is harder on High BPM
            double effectiveStrainTime = Math.Min(osuCurrObj.StrainTime, DifficultyCalculationUtils.BPMToMilliseconds(175, 4)); // Don't nerf BPM below 175 to avoid nerfing alt maps
            flowDifficulty *= (effectiveStrainTime / (effectiveStrainTime - 15));

            double angleBonus = 0;

            if (osuCurrObj.AngleSigned != null && osuLast0Obj.AngleSigned != null && osuLast1Obj.AngleSigned != null)
            {

                double angleChangeBonus = CalculateFlowAngleChangeBonus(current);
                double acuteAngleBonus = CalculateFlowAcuteAngleBonus(current);

                double overlappedNotesWeight = 1;

                if (current.Index > 2)
                {
                    double o1 = getOverlapness(osuCurrObj, osuLast0Obj);
                    double o2 = getOverlapness(osuCurrObj, osuLast1Obj);
                    double o3 = getOverlapness(osuLast0Obj, osuLast1Obj);

                    overlappedNotesWeight = 1 - o1 * o2 * o3;
                }


                if (osuCurrObj.Index >= 4)
                {
                    var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);

                    // Angle is constant
                    double angleCurr = osuCurrObj.Angle ?? 0;
                    double angleLast = osuLast1Obj.Angle ?? 0;
                    double angleLastLast = osuLast2Obj.Angle ?? 0;

                    double deltaAngle = Math.Abs(angleLast - angleLastLast);
                    double isSameAngle = DifficultyCalculationUtils.Smoothstep(deltaAngle, 0.25, 0.15);

                    double currAngleBonus = AimEvaluator.CalcAcuteAngleBonus(angleCurr);
                    double prevAngleBonus = AimEvaluator.CalcAcuteAngleBonus(angleLastLast);

                    double angleBonusDifference = currAngleBonus > 0 ? Math.Clamp(prevAngleBonus / currAngleBonus, 0, 1) : 1;

                    // Or there was no stream before
                    double timeDifferenceFactor = DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLast1Obj.StrainTime * 0.75, osuLast1Obj.StrainTime * 0.55);

                    //double beforeNerf = angleChangeBonus + acuteAngleBonus;

                    acuteAngleBonus *= 1 - isSameAngle * (1 - angleBonusDifference);
                    angleChangeBonus *= 1 - Math.Max(isSameAngle * (1 - prevAngleBonus), timeDifferenceFactor);

                    //double nerf = beforeNerf - angleChangeBonus + acuteAngleBonus;

                    //if (osuCurrObj.StrainTime < 100 && osuLastObj.StrainTime < 100 && nerf > 0.5) Console.WriteLine($"{T(osuCurrObj.BaseObject.StartTime)}: {nerf}");
                }

                // Don't apply both angle change and acute angle bonus at the same time if change is consistent
                double angleChangeCurrent = Math.Abs((double)(osuCurrObj.AngleSigned - osuLast0Obj.AngleSigned));
                double angleChangePrevious = Math.Abs((double)(osuLast0Obj.AngleSigned - osuLast1Obj.AngleSigned));
                double angleChangeBonusDifference = Math.Abs(angleChangePrevious - angleChangeCurrent);
                double angleChangeConsistency = DifficultyCalculationUtils.Smoothstep(angleChangeBonusDifference, 0.2, 0.1);

                double largerBonus = Math.Max(angleChangeBonus, acuteAngleBonus);
                double summedBonus = angleChangeBonus + acuteAngleBonus;

                angleBonus = double.Lerp(summedBonus, largerBonus, angleChangeConsistency) * overlappedNotesWeight;
            }

            double velocityChangeBonus = CalculateFlowVelocityChangeBonus(current);

            flowDifficulty += angleBonus + velocityChangeBonus;
            flowDifficulty *= flowMultiplier;

            if (osuLast0Obj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderBonus = osuLast0Obj.TravelDistance / osuLast0Obj.TravelTime;
                flowDifficulty += sliderBonus * AimEvaluator.SLIDER_MULTIPLIER;
            }

            return flowDifficulty;
        }

        public static string T(double t)
        {
            int seconds = (int)(t / 1000);
            int minutes = seconds / 60;
            return $"{minutes}m {seconds - minutes * 60}s {t % 1000}ms";
        }

        private static double getOverlapness(OsuDifficultyHitObject odho1, OsuDifficultyHitObject odho2)
        {
            OsuHitObject o1 = (OsuHitObject)odho1.BaseObject, o2 = (OsuHitObject)odho2.BaseObject;

            double distance = Vector2.Distance(o1.StackedPosition, o2.StackedPosition);
            double radius = o1.Radius * 0.85; // We want to decrease radius because you can't cheese with very small space

            if (distance >= radius * 2)
                return 0;
            if (distance <= radius)
                return 1;
            return 1 - Math.Pow((distance - radius) / radius, 2);
        }

        public static double CalculateFlowAcuteAngleBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            if (osuCurrObj.Angle == null)
                return 0;

            double currAngle = osuCurrObj.Angle.Value;

            double acuteAngleBonus = AimEvaluator.CalcAcuteAngleBonus(currAngle);
            acuteAngleBonus *= Math.Min(osuCurrObj.LazyJumpDistance, osuLastObj.LazyJumpDistance) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime);

            return acuteAngleBonus;
        }

        public static double CalculateFlowAngleChangeBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            if (osuCurrObj.AngleSigned == null || osuLastObj.AngleSigned == null)
                return 0;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            double currAngle = osuCurrObj.AngleSigned.Value;
            double lastAngle = osuLastObj.AngleSigned.Value;

            double minVelocity = Math.Min(currVelocity, prevVelocity);
            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * minVelocity;

            return angleChangeBonus;
        }

        public static double CalculateFlowVelocityChangeBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;
            double prevPrevVelocity = osuLastLastObj.LazyJumpDistance / osuLastLastObj.StrainTime;

            double minVelocity = Math.Min(currVelocity, prevVelocity);
            double maxVelocity = Math.Max(currVelocity, prevVelocity);

            double deltaVelocity = maxVelocity - minVelocity;
            double deltaPrevVelocity = Math.Abs(prevVelocity - prevPrevVelocity);

            // Don't buff slight consistent changes
            if (minVelocity > 0)
                deltaVelocity -= Math.Min(deltaVelocity, deltaPrevVelocity) * DifficultyCalculationUtils.ReverseLerp(Math.Max(deltaVelocity, deltaPrevVelocity), minVelocity * 0.3, minVelocity * 0.2);

            // Don't buff velocity increase if previous note was slower
            if (currVelocity > prevVelocity)
                deltaVelocity *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLastObj.StrainTime * 0.55, osuLastObj.StrainTime * 0.75);

            if (deltaVelocity > minVelocity * 2)
            {
                double rescaledBonus = deltaVelocity - minVelocity * 2;
                return minVelocity * 2 + Math.Sqrt(2 * rescaledBonus + 1) - 1;
            }

            return deltaVelocity;
        }

        public static double IdentifyComfyCircluarFlow(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 4 || current.Previous(0).BaseObject is Spinner)
                return 0;

            double totalComfyness = 1.0;

            OsuDifficultyHitObject osuCurrObj = (OsuDifficultyHitObject)current;

            double prevAngle = osuCurrObj.AngleSigned ?? 0;
            double prevVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocityChange = double.NaN;

            for (int i = 0; i < 3; i++)
            {
                var relevantObj = (OsuDifficultyHitObject)current.Previous(i);

                double currAngle = relevantObj.AngleSigned ?? 0;
                double currAngleChange = Math.Abs(currAngle - prevAngle);

                double currVelocity = relevantObj.LazyJumpDistance / relevantObj.StrainTime;
                double currVelocityChange = currVelocity / prevVelocity;
                double normalizedVelocityChange = currVelocityChange >= 1 ? currVelocityChange : 1.0 / currVelocityChange;
                double accelerationChange = Math.Abs(currVelocityChange - prevVelocityChange);

                double angleFactor = DifficultyCalculationUtils.Smoothstep(Math.Abs(currAngle), Math.PI * 0.55, Math.PI * 0.75);
                double angleChangeFactor = DifficultyCalculationUtils.Smoothstep(currAngleChange, 0.45, 0.3);
                double velocityChangeFactor = double.IsNaN(normalizedVelocityChange) ? 1.0 : DifficultyCalculationUtils.Smoothstep(normalizedVelocityChange, 1.4, 1.25);
                double accelerationChangeFactor = double.IsNaN(accelerationChange) ? 1.0 : DifficultyCalculationUtils.Smoothstep(accelerationChange, 0.3, 0.2);

                double instantComfyness = angleFactor * angleChangeFactor * velocityChangeFactor * accelerationChangeFactor;
                totalComfyness *= instantComfyness;

                prevVelocity = currVelocity;
                prevVelocityChange = currVelocityChange;
                prevAngle = currAngle;
            }

            return totalComfyness;
        }
    }
}
