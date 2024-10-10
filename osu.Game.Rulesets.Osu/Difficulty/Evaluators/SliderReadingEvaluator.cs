// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using HidSharp.Reports.Units;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class SliderReadingEvaluator
    {
        private const double slider_shape_reading_multiplier = 4;

        /// <summary>
        /// Evaluates the difficulty of tapping the current object, based on:
        /// </summary>
        public static double EvaluateDifficultyOf(Slider slider)
        {
            double complexityBonus = calculateSliderShapeComplexity(slider);
            double pointUnpredictability = calculateSliderPointUnpredictability(slider);

            double difficulty = complexityBonus * (1 + pointUnpredictability);

            return difficulty;
        }

        /// <summary>
        /// Increase from 0 on point1 to 1 on point2
        /// </summary>
        /// <param name="value"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private static double sinusCurve(double value, double point1, double point2)
        {
            if (value < point1) return 0.0;
            if (value > point2) return 1.0;

            return (1 - Math.Cos((value - point1) * Math.PI / (point2 - point1))) / 2; // grows from 0 to 1 as similarity increase from point1 to point2
        }

        private static Vector2 positionWithRepeats(double relativeTime, Slider slider)
        {
            double progress = relativeTime / slider.SpanDuration;
            if (slider.RepeatCount % 2 == 1)
                progress = 1 - progress; // revert if odd number of repeats
            return slider.Position + slider.Path.PositionAt(progress);
        }

        private static Vector2 interpolate(Vector2 start, Vector2 end, float progress)
            => start + (end - start) * progress;

        private static double interpolate(double start, double end, double progress)
            => start + (end - start) * progress;

        private static double getCurveComfort(Vector2 start, Vector2 middle, Vector2 end)
        {
            float deltaDistance = Math.Abs(Vector2.Distance(start, middle) - Vector2.Distance(middle, end));
            float scaleDistance = Vector2.Distance(start, end) / 4;
            float result = Math.Min(deltaDistance / scaleDistance, 1);
            return 1 - (double)result;
        }

        private static double getCurveDegree(Vector2 start, Vector2 middle, Vector2 end)
        {
            Vector2 middleInterpolated = interpolate(start, end, 0.5f);
            float distance = Vector2.Distance(middleInterpolated, middle);

            float scaleDistance = Vector2.Distance(start, end);
            float maxComfortDistance = scaleDistance / 2;

            float result = Math.Clamp((distance - maxComfortDistance) / scaleDistance, 0, 1);
            return 1 - (double)result;
        }
        private static double calculateSliderShapeComplexity(Slider slider)
        {
            if (slider.SpanDuration == 0) return 0;

            double minFollowRadius = slider.Radius;
            double maxFollowRadius = slider.Radius * 2.4;
            double deltaFollowRadius = maxFollowRadius - minFollowRadius;

            OsuHitObject head;
            if (slider.RepeatCount == 0)
                head = (OsuHitObject)slider.NestedHitObjects[0];
            else
                head = (OsuHitObject)slider.NestedHitObjects.Where(o => o is SliderRepeat).Last();

            var tail = (OsuHitObject)slider.NestedHitObjects[^1];

            double numberOfUpdates = Math.Ceiling(2 * slider.Path.Distance / slider.Radius);
            double deltaT = slider.SpanDuration / numberOfUpdates;

            double relativeTime = 0;
            double middleTime = (head.StartTime + tail.StartTime) / 2 - head.StartTime;
            var middlePos = positionWithRepeats(middleTime, slider);

            double lineBonus = 0;
            double curveBonus = 0;

            for (; relativeTime <= middleTime; relativeTime += deltaT)
            {
                // calculating position of the normal path
                Vector2 ballPosition = positionWithRepeats(relativeTime, slider);

                // calculation position of the line path
                float localProgress = (float)(relativeTime / (slider.EndTime - head.StartTime));
                localProgress = Math.Clamp(localProgress, 0, 1);

                Vector2 linePosition = interpolate(head.Position, tail.Position, localProgress);

                // buff scales from 0 to 1 when slider follow distance is changing from 1.0x to 2.4x
                double continousLineBuff = (Vector2.Distance(ballPosition, linePosition) - minFollowRadius) / deltaFollowRadius;
                continousLineBuff = Math.Clamp(continousLineBuff, 0, 1) * deltaT;

                // calculation position of the curvy path
                localProgress = (float)(relativeTime / middleTime);
                localProgress = Math.Clamp(localProgress, 0, 1);

                Vector2 curvyPosition = interpolate(head.Position, middlePos, localProgress);

                // buff scales from 0 to 1 when slider follow distance is changing from 1.0x to 2.4x
                double continousCurveBuff = (Vector2.Distance(ballPosition, curvyPosition) - minFollowRadius) / deltaFollowRadius;
                continousCurveBuff = Math.Clamp(continousCurveBuff, 0, 1) * deltaT;

                lineBonus += (float)continousLineBuff;
                curveBonus += (float)continousCurveBuff;
            }

            for (; relativeTime <= slider.SpanDuration; relativeTime += deltaT)
            {
                // calculating position of the normal path
                Vector2 ballPosition = positionWithRepeats(relativeTime, slider);

                // calculation position of the line path
                float localProgress = (float)(relativeTime / (slider.EndTime - head.StartTime));
                localProgress = Math.Clamp(localProgress, 0, 1);

                Vector2 linePosition = interpolate(head.Position, tail.Position, localProgress);

                // buff scales from 0 to 1 when slider follow distance is changing from 1.0x to 2.4x
                double continousLineBuff = (Vector2.Distance(ballPosition, linePosition) - minFollowRadius) / deltaFollowRadius;
                continousLineBuff = Math.Clamp(continousLineBuff, 0, 1) * deltaT;

                // calculation position of the curvy path
                localProgress = (float)((relativeTime - middleTime) / (slider.SpanDuration - middleTime));
                localProgress = Math.Clamp(localProgress, 0, 1);

                Vector2 curvyPosition = interpolate(middlePos, tail.Position, localProgress);

                // buff scales from 0 to 1 when slider follow distance is changing from 1.0x to 2.4x
                double continousCurveBuff = (Vector2.Distance(ballPosition, curvyPosition) - minFollowRadius) / deltaFollowRadius;
                continousCurveBuff = Math.Clamp(continousCurveBuff, 0, 1) * deltaT;

                lineBonus += continousLineBuff;
                curveBonus += continousCurveBuff;
            }

            double comfortableCurveFactor = getCurveComfort(head.Position, middlePos, tail.Position);
            double curveDegreeFactor = getCurveDegree(head.Position, middlePos, tail.Position);

            curveBonus = interpolate(lineBonus, curveBonus, comfortableCurveFactor * curveDegreeFactor);

            lineBonus *= slider_shape_reading_multiplier / slider.SpanDuration;
            curveBonus *= slider_shape_reading_multiplier / slider.SpanDuration;

            return Math.Min(lineBonus, curveBonus);
        }

        private static double calculateSliderPointUnpredictability(Slider slider)
        {
            double prevAngle = double.NaN;
            double prevDeltaTime = double.NaN;

            OsuHitObject? prevObj0 = null, prevObj1 = null;
            Vector2? prevObjPos0 = null, prevObjPos1 = null;

            double totalBonus = 0;
            int objectCount = 0;

            foreach (var obj in slider.NestedHitObjects)
            {
                // Calculate curr

                OsuHitObject currObj = (OsuHitObject)obj;
                Vector2 currObjPos = currObj.StackedPosition;

                if (prevObj0 == null || prevObjPos0 == null)
                {
                    prevObj0 = currObj;
                    prevObjPos0 = currObjPos;
                    continue;
                }

                double currDistance = Vector2.Distance(currObjPos, (Vector2)prevObjPos0);
                double currDeltaTime = currObj.StartTime - prevObj0.StartTime;

                double currVelocity = currDistance / currDeltaTime;

                if (prevObj1 == null || prevObjPos1 == null)
                {
                    prevObj1 = prevObj0;
                    prevObj0 = currObj;

                    prevObjPos1 = prevObjPos0;
                    prevObjPos0 = currObjPos;

                    prevDeltaTime = currDeltaTime;
                    continue;
                }

                // Angle change bonus

                double currAngle = calculateAngleSigned((Vector2)prevObjPos1, (Vector2)prevObjPos0, currObjPos);

                double angleChangeBonus = 0;
                if (!double.IsNaN(prevAngle))
                {
                    angleChangeBonus = sinusCurve(Math.Abs(currAngle - prevAngle), 0.2, 0.7);

                    // Punish wide angles
                    angleChangeBonus *= 1 - sinusCurve(Math.Abs(currAngle), Math.PI / 2, Math.PI * 5 / 6);
                }

                // Velocity change bonus

                double prevDistance = Vector2.Distance((Vector2)prevObjPos0, (Vector2)prevObjPos1);
                double prevVelocity = prevDistance / prevDeltaTime;

                (double prevCircularDistance, double currCircularDistance) = calculateCircularDistance((Vector2)prevObjPos1, (Vector2)prevObjPos0, currObjPos);
                double prevCircularVelocity = prevCircularDistance / prevDeltaTime;
                double currCircularVelocity = currCircularDistance / currDeltaTime;

                double velocityChangeBonusLinear = sinusCurve(Math.Abs(currVelocity - prevVelocity), 0.2, 0.6);
                double velocityChangeBonusCircular = sinusCurve(Math.Abs(currCircularVelocity - prevCircularVelocity), 0.2, 0.6);

                double velocityChangeBonus = Math.Min(velocityChangeBonusLinear, velocityChangeBonusCircular);

                totalBonus += (angleChangeBonus + velocityChangeBonus) * 2;
                objectCount += 1;

                // Assign to prev

                prevObj1 = prevObj0;
                prevObj0 = currObj;

                prevObjPos1 = prevObjPos0;
                prevObjPos0 = currObjPos;

                prevVelocity = currVelocity;
                prevDeltaTime = currDeltaTime;
                prevAngle = currAngle;

                if (obj is SliderEndCircle || obj is SliderRepeat) break;
            }

            if (objectCount == 0)
                return 0;

            return Math.Min(1, totalBonus / objectCount);
        }

        private static (double prev, double curr) calculateCircularDistance(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            static double sqr(double x) => x * x;
            double currDist = Vector2.Distance(p2, p3);
            double prevDist = Vector2.Distance(p1, p2);
            double? angle = Math.Abs(calculateAngleSigned(p1, p2, p3));

            double triangleArea = currDist * prevDist * Math.Sin(angle ?? 0) / 2;
            if (triangleArea == 0)
                return (prevDist, currDist);

            double thirdSide = Math.Sqrt(sqr(currDist) + sqr(prevDist) - 2 * currDist * prevDist * Math.Cos(angle ?? Math.PI));
            double circleRadius = currDist * prevDist * thirdSide / 4 / triangleArea;

            double circularAnglePrev = Math.Abs(calculateAngleSigned(p1, p3, p2));
            double circularAngleCurr = Math.Abs(calculateAngleSigned(p3, p1, p2));

            double flowDistancePrev = circularAnglePrev / Math.PI * circleRadius;
            double flowDistanceCurr = circularAngleCurr / Math.PI * circleRadius;

            return (flowDistancePrev, flowDistanceCurr);
        }

        private static double calculateAngleSigned(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 v1 = p3 - p2;
            Vector2 v2 = p1 - p2;

            float dot = Vector2.Dot(v1, v2);
            float det = v1.X * v2.Y - v1.Y * v2.X;

            return Math.Atan2(det, dot);
        }

        private static double calculateAngleAbs(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 v1 = p2 - p1;
            Vector2 v2 = p2 - p3;

            float dot = v1.X * v2.X + v1.Y * v2.Y;
            float modSum = v1.Length * v2.Length;

            return Math.Acos(dot / modSum);
        }
    }
}
