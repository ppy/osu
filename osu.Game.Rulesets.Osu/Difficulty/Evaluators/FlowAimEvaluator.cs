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
        private static double flowMultiplier => 1.18;

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
                flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.65 - 0.5 * comfyness);
            }
            else
            {
               flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 1);
            }

            // Flow aim is harder on High BPM
            double effectiveStrainTime = Math.Min(osuCurrObj.StrainTime, DifficultyCalculationUtils.BPMToMilliseconds(175, 4)); // Don't nerf BPM below 175 to avoid nerfing alt maps
            flowDifficulty *= (effectiveStrainTime / (effectiveStrainTime - 7));

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
            if (current.BaseObject is Spinner || current.Index <= 2 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);
            var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);
            var osuLast3Obj = (OsuDifficultyHitObject)current.Previous(3);

            const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLast0Obj.LazyJumpDistance / osuLast0Obj.StrainTime;
            double prev1Velocity = osuLast1Obj.LazyJumpDistance / osuLast1Obj.StrainTime;

            double minVelocity = Math.Min(currVelocity, prevVelocity);
            double maxVelocity = Math.Max(currVelocity, prevVelocity);

            double deltaVelocity = maxVelocity - minVelocity;
            double deltaPrevVelocity = Math.Abs(prevVelocity - prev1Velocity);

            // Don't buff slight consistent changes
            if (minVelocity > 0)
                deltaVelocity -= Math.Min(deltaVelocity, deltaPrevVelocity) * DifficultyCalculationUtils.ReverseLerp(Math.Max(deltaVelocity, deltaPrevVelocity), minVelocity * 0.3, minVelocity * 0.2);

            // Don't buff velocity increase if previous note was slower
            if (currVelocity > prevVelocity)
                deltaVelocity *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLast0Obj.StrainTime * 0.55, osuLast0Obj.StrainTime * 0.75);

            double currDistance = Math.Max(osuCurrObj.LazyJumpDistance, 0.01);
            double prevDistance = Math.Max(osuLast0Obj.LazyJumpDistance, 0.01);
            double prev1Distance = Math.Max(osuLast1Obj.LazyJumpDistance, 0.01);
            double prev2Distance = Math.Max(osuLast2Obj?.LazyJumpDistance ?? prev1Distance, 0.01);

            // Decrease buff on cutstreams
            double distanceSimilarityFactor = DifficultyCalculationUtils.ReverseLerpTwoDirectional(prev1Distance, prev2Distance, 0.8, 0.95);

            // Don't sure if it's needed
            //double angleFactor = DifficultyCalculationUtils.Smoothstep(osuLast1Obj.Angle ?? 0, Math.PI * 0.55, Math.PI * 0.75)
            //    * DifficultyCalculationUtils.Smoothstep(osuLast2Obj?.Angle ?? 0, Math.PI * 0.55, Math.PI * 0.75);

            double distanceFactor = 0.5 + 0.5 * DifficultyCalculationUtils.ReverseLerp(Math.Max(prev1Distance, prev2Distance), diameter * 1.5, diameter * 0.75);

            deltaVelocity *= 1 - 0.5 * distanceSimilarityFactor * distanceFactor;

            // Decrease buff on doubles that go back and forth
            // Add radius to account for distance potenitally being very small
            double distanceSimilarity1 = DifficultyCalculationUtils.ReverseLerpTwoDirectional(currDistance + radius, prev1Distance + radius, 0.7, 0.9);
            double distanceSimilarity2 = DifficultyCalculationUtils.ReverseLerpTwoDirectional(prevDistance + radius, prev2Distance + radius, 0.7, 0.9);

            // Check the direction of doubles
            double directionFactor = 1.0;
            if (osuLast3Obj != null)
            {
                Vector2 p1 = ((OsuHitObject)osuCurrObj.BaseObject).StackedPosition;
                Vector2 p2 = ((OsuHitObject)osuLast1Obj.BaseObject).StackedPosition;
                Vector2 p3 = ((OsuHitObject)osuLast3Obj.BaseObject).StackedPosition;

                Vector2 v1 = p3 - p2;
                Vector2 v2 = p1 - p2;

                float dot = Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                double angle = Math.Abs(Math.Atan2(det, dot));

                directionFactor = DifficultyCalculationUtils.ReverseLerp(angle, 3 * Math.PI / 2, Math.PI / 2);
            }

            deltaVelocity *= 1 - distanceSimilarity1 * distanceSimilarity2 * directionFactor;

            if (deltaVelocity > minVelocity * 2)
            {
                double rescaledBonus = deltaVelocity - minVelocity * 2;
                return minVelocity * 2 + Math.Sqrt(2 * rescaledBonus + 1) - 1;
            }

            return deltaVelocity;
        }

        public static double IdentifyCutStream(DifficultyHitObject current)
        {
            return 0;
        }

        public static double IdentifyComfyCircluarFlow(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 4 || current.Previous(0).BaseObject is Spinner)
                return 0;

            double totalComfyness = 1.0;

            OsuDifficultyHitObject osuCurrObj = (OsuDifficultyHitObject)current;

            double prevAngle = osuCurrObj.AngleSigned ?? 0;
            double prevAngleChange = 0;
            double prevVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocityChange = double.NaN;

            // It's allowed to get two angle change without triggering comfyness penalty

            // First one is normal direction change
            double angleLeniency = 1.0;

            // Second one is the S type of movement where clockwise is changing to counterclockwise
            double angleChangeLeniency = 1.0;

            for (int i = 0; i < 3; i++)
            {
                var relevantObj = (OsuDifficultyHitObject)current.Previous(i);

                double currAngle = relevantObj.AngleSigned ?? 0;

                if (angleLeniency > 0)
                {
                    double currAngleAbs = Math.Abs(currAngle);
                    double prevAngleAbs = Math.Abs(prevAngle);

                    double potentialLeniency = Math.Max(currAngleAbs, prevAngleAbs) - currAngleAbs;

                    double usedLeniency = Math.Min(angleLeniency * Math.PI, potentialLeniency);
                    currAngle = (currAngleAbs + usedLeniency) * Math.Sign(currAngle);
                    angleLeniency -= Math.Min(usedLeniency, 1);
                }

                double currAngleChange = Math.Abs(currAngle - prevAngle);
                double relevantAngleChange = currAngleChange;

                if (angleChangeLeniency > 0)
                {
                    double potentialLeniency = currAngleChange - Math.Min(currAngleChange, prevAngleChange);

                    double usedLeniency = Math.Min(angleChangeLeniency * Math.PI, potentialLeniency);
                    relevantAngleChange = Math.Min(relevantAngleChange, 1) * (1 - usedLeniency);
                    angleChangeLeniency -= Math.Min(usedLeniency, 1);
                }

                double currVelocity = relevantObj.LazyJumpDistance / relevantObj.StrainTime;
                double currVelocityChange = currVelocity / prevVelocity;
                double normalizedVelocityChange = currVelocityChange >= 1 ? currVelocityChange : 1.0 / currVelocityChange;
                double accelerationChange = Math.Abs(currVelocityChange - prevVelocityChange);

                double angleFactor = DifficultyCalculationUtils.Smoothstep(Math.Abs(currAngle), Math.PI * 0.55, Math.PI * 0.75);
                double angleChangeFactor = DifficultyCalculationUtils.Smoothstep(relevantAngleChange, 0.45, 0.3);
                double velocityChangeFactor = double.IsNaN(normalizedVelocityChange) ? 1.0 : DifficultyCalculationUtils.Smoothstep(normalizedVelocityChange, 1.4, 1.25);
                double accelerationChangeFactor = double.IsNaN(accelerationChange) ? 1.0 : DifficultyCalculationUtils.Smoothstep(accelerationChange, 0.3, 0.2);

                double instantComfyness = angleFactor * angleChangeFactor * velocityChangeFactor * accelerationChangeFactor;
                totalComfyness *= instantComfyness;

                prevVelocity = currVelocity;
                prevVelocityChange = currVelocityChange;
                prevAngle = currAngle;
                prevAngleChange = currAngleChange;
            }

            return totalComfyness;
        }
    }
}
