﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        private static double flowMultiplier => 1.14;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);
            var osuLast2Obj = (OsuDifficultyHitObject?)current.Previous(2);
            var osuLast3Obj = (OsuDifficultyHitObject?)current.Previous(3);

            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Start with velocity
            double velocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;

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
                // Decrease spacing if patterns are comfy
                double comfyness = IdentifyComfyFlow(current);

                // Change those 2 power coeficients to control amount of buff high spaced flow aim has for comfy/uncomfy patterns
                flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.75 - 0.55 * comfyness);
            }
            else
            {
                // Decrease power here if you want to buff low-spaced flow aim
                flowDifficulty *= Math.Pow(osuCurrObj.LazyJumpDistance / diameter, 0.8);
            }

            // Flow aim is harder on High BPM
            // Increase multiplier in the beginning to buff all the scaling
            // Increase power to increase buff for spaced speedflow
            // Increase number in the divisor to make steeper scaling with bpm
            flowDifficulty += 2.2 * (Math.Pow(osuCurrObj.LazyJumpDistance, 0.7) / osuCurrObj.StrainTime) * (osuCurrObj.StrainTime / (osuCurrObj.StrainTime - 12) - 1);

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

                // Don't apply both angle change and acute angle bonus at the same time if change is consistent
                double angleChangeCurr = (double)(osuCurrObj.AngleSigned - osuLast0Obj.AngleSigned);
                double angleChangePrev = (double)(osuLast1Obj.AngleSigned - osuLast0Obj.AngleSigned);
                double angleChangePrev1 = (double)(osuLast1Obj.AngleSigned - osuLast2Obj?.AngleSigned ?? 0);

                double angleChangeConsistencyFactor = DifficultyCalculationUtils.Smoothstep(Math.Abs(angleChangePrev - angleChangeCurr), 0.2, 0.1)
                    * DifficultyCalculationUtils.Smoothstep(Math.Abs(angleChangePrev1 - angleChangePrev), 0.2, 0.1);

                // Assume the angle change is consistent if some of the notes are slower
                double strainTimeDifferenceFactor = calculateSlowerNoteFactor(osuCurrObj, osuLast0Obj);
                strainTimeDifferenceFactor *= calculateSlowerNoteFactor(osuLast0Obj, osuLast1Obj);
                strainTimeDifferenceFactor *= calculateSlowerNoteFactor(osuLast1Obj, osuLast2Obj);
                strainTimeDifferenceFactor *= calculateSlowerNoteFactor(osuLast2Obj, osuLast3Obj);
                angleChangeConsistencyFactor = 1 - strainTimeDifferenceFactor * (1 - angleChangeConsistencyFactor);

                double largerBonus = Math.Max(angleChangeBonus, acuteAngleBonus);
                double summedBonus = angleChangeBonus + acuteAngleBonus;

                angleBonus = double.Lerp(summedBonus, largerBonus, angleChangeConsistencyFactor) * overlappedNotesWeight;

                //angleBonus = Math.Max(angleChangeBonus, acuteAngleBonus) * overlappedNotesWeight;
            }

            double velocityChangeBonus = CalculateFlowVelocityChangeBonus(current);
            flowDifficulty += angleBonus + velocityChangeBonus;

            flowDifficulty *= flowMultiplier;

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
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);
            var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);

            if (osuCurrObj.Angle == null)
                return 0;

            double currAngle = (double)osuCurrObj.Angle;
            double last1Angle = osuLast1Obj.Angle ?? 0;
            double last2Angle = osuLast2Obj?.Angle ?? 0;

            double currAngleBonus = AimEvaluator.CalcAcuteAngleBonus(currAngle);
            double prevAngleBonus = AimEvaluator.CalcAcuteAngleBonus(last2Angle);

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double acuteAngleBonus = currVelocity * currAngleBonus;

            // Nerf acute angle if previous notes were slower
            // IMPORTANT INFORMATION: removing this limitation buffs many alt maps
            // BUT it also  buffs ReLief. So it's should be explored how to keep this buff for actually hard patterns but not for ReLief
            acuteAngleBonus *= DifficultyCalculationUtils.ReverseLerp(osuCurrObj.StrainTime, osuLast0Obj.StrainTime * 0.55, osuLast0Obj.StrainTime * 0.75);

            // Decrease angle bonus if angle changes are slower than 1 in 4 notes
            double deltaAngle = Math.Abs(last1Angle - last2Angle);
            double isSameAngle = DifficultyCalculationUtils.Smoothstep(deltaAngle, 0.25, 0.15); // =1 if there's no angle change

            double angleBonusDifference = currAngleBonus > 0 ? Math.Clamp(prevAngleBonus / currAngleBonus, 0, 1) : 1;

            // Decrease buffs from angle bonus if it's not repeating too often
            // Multiply nerf by difference in bonus to not nerf repeating high angle bonuse
            acuteAngleBonus *= 1 - 0.5 * isSameAngle * (1 - angleBonusDifference);

            return acuteAngleBonus;
        }

        // This bonus accounts for flow aim being harder when angle is changing. There's extra bonus for changes occuring more often than once in 4 notes.
        public static double CalculateFlowAngleChangeBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLast0Obj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);
            var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);

            if (osuCurrObj.AngleSigned == null || osuLast0Obj.AngleSigned == null)
                return 0;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLast0Obj.LazyJumpDistance / osuLast0Obj.StrainTime;

            double currAngle = osuCurrObj.AngleSigned.Value;
            double lastAngle = osuLast0Obj.AngleSigned.Value;

            // Use reasonable value between min and curr velocity as a base
            double baseVelocity = double.Lerp(Math.Min(currVelocity, prevVelocity), currVelocity, 0.25);
            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * baseVelocity;

            // Remove angle change if previous 2 notes were slower
            // IMPORTANT INFORMATION: removing this limitation significantly buffs almost all tech, alt, underweight maps in general
            // BUT it also very significantly buffs ReLief. So it's should be explored how to keep this buff for actually hard patterns but not for ReLief
            angleChangeBonus *= DifficultyCalculationUtils.ReverseLerp(osuCurrObj.StrainTime, osuLast0Obj.StrainTime * 0.55, osuLast0Obj.StrainTime * 0.75);
            angleChangeBonus *= DifficultyCalculationUtils.ReverseLerp(osuCurrObj.StrainTime, osuLast1Obj.StrainTime * 0.55, osuLast1Obj.StrainTime * 0.75);

            double last1Angle = osuLast1Obj.Angle ?? 0;
            double last2Angle = osuLast2Obj?.Angle ?? 0;

            double deltaAngle = Math.Abs(last1Angle - last2Angle);
            double isSameAngle = DifficultyCalculationUtils.Smoothstep(deltaAngle, 0.25, 0.15); // =1 if there's no angle change

            double prevAngleBonus = AimEvaluator.CalcAcuteAngleBonus(last2Angle);

            // Decrease buffs from angle bonus if it's not repeating too often
            // Multiply nerf by difference in bonus to not nerf repeating high angle bonuse
            angleChangeBonus *= 1 - 0.5 * isSameAngle * (1 - prevAngleBonus);

            return angleChangeBonus;
        }

        // This bonus accounts for the fact that changing velocity makes flow aim harder.
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

            double minVelocity = Math.Min(currVelocity, prevVelocity);
            double maxVelocity = Math.Max(currVelocity, prevVelocity);

            double deltaVelocity = maxVelocity - minVelocity;

            // Buff acceleration 2 times more than decceleration
            if (currVelocity > prevVelocity) deltaVelocity *= 2;

            // Don't buff velocity increase if previous note was slower
            if (currVelocity > prevVelocity)
                deltaVelocity *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLast0Obj.StrainTime * 0.55, osuLast0Obj.StrainTime * 0.75);

            double currDistance = osuCurrObj.LazyJumpDistance;
            double prevDistance = osuLast0Obj.LazyJumpDistance;
            double prev1Distance = osuLast1Obj.LazyJumpDistance;
            double prev2Distance = osuLast2Obj?.LazyJumpDistance ?? 0;

            // If previously there was slow flow pattern - sudden velocity change is much easier because you could flow faster to give yourself more time
            // Add radius to account for distance potenitally being very small
            double distanceSimilarityFactor = DifficultyCalculationUtils.ReverseLerp(prev1Distance + radius, (prev2Distance + radius) * 0.8, (prev2Distance + radius) * 0.95);
            double distanceFactor = 0.5 + 0.5 * DifficultyCalculationUtils.ReverseLerp(Math.Max(prev1Distance, prev2Distance), diameter * 1.5, diameter * 0.75);
            // There also should be something like angleFactor, because if it has aim-control difficulty - you can't really speed-up flow aim that easily

            deltaVelocity *= 1 - 0.67 * distanceSimilarityFactor * distanceFactor;

            // Decrease buff on doubles that go back and forth, because in this case angle change bonuses account for all added difficulty
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

            // Penalize rhythm change
            deltaVelocity *= DifficultyCalculationUtils.ReverseLerp(osuLast0Obj.StrainTime, osuCurrObj.StrainTime * 0.55, osuCurrObj.StrainTime * 0.75);

            // Don't reward very big differences too much
            if (deltaVelocity > minVelocity * 2)
            {
                double rescaledBonus = deltaVelocity - minVelocity * 2;
                return minVelocity * 2 + Math.Sqrt(2 * rescaledBonus + 1) - 1;
            }

            return deltaVelocity;
        }

        // This function is used to reward high spacing on uncomfy flow outside of direct bonuses.
        public static double IdentifyComfyFlow(DifficultyHitObject current)
        {
            // Check starts from this note before current and ends with current notes
            // Previous 3 notes before are still checked for some adjustments but not relevenat to main check
            const int starting_index = 4;

            if (current.BaseObject is Spinner || current.Index < starting_index || current.Previous(0).BaseObject is Spinner)
                return 0;

            double totalComfyness = 1.0;

            OsuDifficultyHitObject osuLast1Obj = (OsuDifficultyHitObject)current.Previous(starting_index - 1);
            OsuDifficultyHitObject? osuLast2Obj = (OsuDifficultyHitObject)current.Previous(starting_index);
            OsuDifficultyHitObject? osuLast3Obj = (OsuDifficultyHitObject)current.Previous(starting_index + 1);

            double prevAngle = osuLast1Obj.AngleSigned ?? 0;
            double prevAngleChange = 0;

            double prev2Velocity = osuLast3Obj != null ? osuLast3Obj.LazyJumpDistance / osuLast3Obj.StrainTime : double.NaN;
            double prev1Velocity = osuLast2Obj != null ? osuLast2Obj.LazyJumpDistance / osuLast2Obj.StrainTime : double.NaN;
            double prevVelocity = osuLast1Obj.LazyJumpDistance / osuLast1Obj.StrainTime;

            double prevVelocityChange = prevVelocity / prev1Velocity;
            double prev1VelocityChange = prev1Velocity / prev2Velocity;
            double prev2VelocityChange = double.NaN;

            // It's allowed to get two angle change without stream to be considered comfy

            // First one is normal Y type direction change
            double angleLeniency = 1.0;

            // Second one is the S type of movement where clockwise is changing to counterclockwise
            double angleChangeLeniency = 1.0;

            for (int i = starting_index - 2; i >= -1; i--)
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

                double currAngleChange = Math.Pow(Math.Sin((currAngle - prevAngle) / 2), 2);
                double relevantAngleChange = currAngleChange;

                if (angleChangeLeniency > 0)
                {
                    double potentialLeniency = currAngleChange - Math.Min(currAngleChange, prevAngleChange);

                    double usedLeniency = Math.Min(angleChangeLeniency, potentialLeniency);
                    relevantAngleChange *= (1 - usedLeniency);
                    angleChangeLeniency -= usedLeniency;
                }

                double currVelocity = relevantObj.LazyJumpDistance / relevantObj.StrainTime;
                double currVelocityChange = currVelocity / prevVelocity;
                double normalizedVelocityChange = normalizeVelocityChange(currVelocityChange);
                double accelerationChange = Math.Abs(currVelocityChange - prevVelocityChange);

                double angleFactor = DifficultyCalculationUtils.Smoothstep(Math.Abs(currAngle), Math.PI * 0.55, Math.PI * 0.75);
                double angleChangeFactor = DifficultyCalculationUtils.Smoothstep(relevantAngleChange, 0.3, 0.2);

                // Adjust coefs for velocity change factors if velocity wasn't changing before
                // It's using very old velocity change stats because accelaration change is using 3 objects beforehand, so to check previos state it need at least objects 4th and 5th
                double normalVelocityChangeAdjust = 0.15 * DifficultyCalculationUtils.Smoothstep(normalizeVelocityChange(prev1VelocityChange) * normalizeVelocityChange(prev2VelocityChange), 1.2, 1.05);

                double velocityChangeFactor = DifficultyCalculationUtils.Smoothstep(normalizedVelocityChange, 1.25 + normalVelocityChangeAdjust, 1.1 + normalVelocityChangeAdjust);
                double accelerationChangeFactor = double.IsNaN(accelerationChange) ? 1.0 : DifficultyCalculationUtils.Smoothstep(accelerationChange, 0.25 + normalVelocityChangeAdjust, 0.15 + normalVelocityChangeAdjust);

                // Don't look on accelerationChangeFactor on first iteration because it looks for one object behind than other bonuses, and we don't want it
                double instantComfyness = angleFactor * angleChangeFactor * velocityChangeFactor * (i == starting_index - 2 ? 1.0 : accelerationChangeFactor);
                totalComfyness *= instantComfyness;

                prevVelocity = currVelocity;

                prev2VelocityChange = prev1VelocityChange;
                prev1VelocityChange = prevVelocityChange;
                prevVelocityChange = currVelocityChange;

                prevAngle = currAngle;
                prevAngleChange = currAngleChange;
            }

            return totalComfyness;
        }

        private static double calculateSlowerNoteFactor(OsuDifficultyHitObject? osuCurrObj, OsuDifficultyHitObject? osuPrevObj)
        {
            if (osuCurrObj == null || osuPrevObj == null)
                return 0;

            return DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuPrevObj.StrainTime * 0.75, osuPrevObj.StrainTime * 0.95);
        }
        private static double normalizeVelocityChange(double velocityChange) => double.IsNaN(velocityChange) ? 1.0 : velocityChange >= 1 ? velocityChange : 1.0 / velocityChange;
    }
}
