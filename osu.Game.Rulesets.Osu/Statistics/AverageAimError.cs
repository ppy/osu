// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Rulesets.Osu.Statistics
{
    /// <summary>
    /// Displays the unstable rate statistic for a given play.
    /// </summary>
    public partial class AverageAimError : SimpleStatisticItem<double?>
    {
        private readonly List<Vector2> hitPoints = new List<Vector2>();

        /// <summary>
        /// Creates and computes an <see cref="AverageHitError"/> statistic.
        /// </summary>
        /// <param name="hitEvents">Sequence of <see cref="HitEvent"/>s to calculate the unstable rate based on.</param>
        public AverageAimError(IEnumerable<HitEvent> hitEvents)
            : base("Average Aim Error")
        {
            Value = calculateAverageAimError(hitEvents);
        }

        private double? calculateAverageAimError(IEnumerable<HitEvent> hitEvents)
        {
            IEnumerable<HitEvent> hitCircleEvents = hitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle) && e.Result.IsHit());

            double nonMissCount = hitCircleEvents.Count(e => e.Result.IsHit());

            if (nonMissCount == 0)
                return null;

            foreach (var e in hitCircleEvents)
            {
                if (e.LastHitObject == null || e.Position == null)
                    continue;

                addAngleAdjustedPoint(((OsuHitObject)e.LastHitObject).StackedEndPosition, ((OsuHitObject)e.HitObject).StackedEndPosition, e.Position.Value);
            }

            Vector2 averagePosition = new Vector2(hitPoints.Sum(x => x[0]), hitPoints.Sum(x => x[1])) / (float)nonMissCount;

            return averagePosition.Length;
        }

        private void addAngleAdjustedPoint(Vector2 start, Vector2 end, Vector2 hitPoint)
        {
            double angle1 = Math.Atan2(end.Y - hitPoint.Y, hitPoint.X - end.X); // Angle between the end point and the hit point.
            double angle2 = Math.Atan2(end.Y - start.Y, start.X - end.X); // Angle between the end point and the start point.
            double finalAngle = angle2 - angle1; // Angle between start, end, and hit points.

            double distanceFromCenter = (hitPoint - end).Length;

            Vector2 angleAdjustedPoint = new Vector2((float)(Math.Sin(finalAngle) * distanceFromCenter), (float)(Math.Cos(finalAngle) * distanceFromCenter));

            hitPoints.Add(angleAdjustedPoint);
        }

        protected override string DisplayValue(double? value) => value == null ? "(not available)" : $"{Math.Abs(value.Value):N2} osu! pixels from center";
    }
}
