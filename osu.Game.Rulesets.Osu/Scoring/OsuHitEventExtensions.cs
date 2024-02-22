// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public static class OsuHitEventExtensions
    {
        public static double? CalculateAimError(this IEnumerable<HitEvent> hitEvents)
        {
            IEnumerable<HitEvent> hitCircleEvents = hitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle)).ToList();

            Vector2 averageHitError = hitCircleEvents.CalculateAverageAimError()!.Value;

            int eventCount = 0;
            double varianceSum = 0;

            foreach (var e in hitCircleEvents)
            {
                if (e.Position == null)
                    continue;

                eventCount += 1;
                varianceSum += (e.CalcAngleAdjustedPoint() - averageHitError ?? new Vector2(0, 0)).LengthSquared;
            }

            if (eventCount == 0)
                return null;

            // We don't get data for miss locations, so we estimate the total variance using the Rayleigh distribution.
            // Deriving the Rayleigh distribution in this form results in a 2 in the denominator,
            // but it is removed to take the variance across both axes, instead of across just one.
            double variance = varianceSum / eventCount;

            return Math.Sqrt(variance) * 10;
        }

        public static Vector2? CalculateAverageAimError(this IEnumerable<HitEvent> hitEvents)
        {
            IEnumerable<HitEvent> hitCircleEvents = hitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle)).ToList();

            int eventCount = 0;
            Vector2 sumOfPointVectors = new Vector2(0, 0);

            foreach (var e in hitCircleEvents)
            {
                if (e.LastHitObject == null || e.Position == null)
                    continue;

                eventCount += 1;
                sumOfPointVectors += CalcAngleAdjustedPoint(e) ?? new Vector2(0, 0);
            }

            if (eventCount == 0)
                return null;

            Vector2 averagePosition = sumOfPointVectors / eventCount;

            return averagePosition;
        }

        public static Vector2? CalcAngleAdjustedPoint(this HitEvent hitEvent)
        {
            if (hitEvent.LastHitObject is null || hitEvent.Position is null)
                return null;

            Vector2 start = ((OsuHitObject)hitEvent.LastHitObject!).StackedEndPosition;
            Vector2 end = ((OsuHitObject)hitEvent.HitObject).StackedEndPosition;
            Vector2 hitPoint = hitEvent.Position!.Value;

            double angle1 = Math.Atan2(end.Y - hitPoint.Y, hitPoint.X - end.X); // Angle between the end point and the hit point.
            double angle2 = Math.Atan2(end.Y - start.Y, start.X - end.X); // Angle between the end point and the start point.
            double finalAngle = angle2 - angle1; // Angle between start, end, and hit points.

            double distanceFromCenter = (hitPoint - end).Length;

            Vector2 angleAdjustedPoint = new Vector2((float)(Math.Cos(finalAngle) * distanceFromCenter), (float)(Math.Sin(finalAngle) * distanceFromCenter));

            return angleAdjustedPoint;
        }
    }
}
