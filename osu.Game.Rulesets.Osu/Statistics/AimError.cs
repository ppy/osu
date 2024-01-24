// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Rulesets.Osu.Statistics
{
    /// <summary>
    /// Displays the unstable rate statistic for a given play.
    /// </summary>
    public partial class AimError : SimpleStatisticItem<double?>
    {
        private readonly List<Vector2> hitPoints = new List<Vector2>();

        /// <summary>
        /// Creates and computes an <see cref="AimError"/> statistic.
        /// </summary>
        public AimError(IEnumerable<HitEvent> hitEvents, IBeatmap playableBeatmap)
            : base("Aim Error")
        {
            Value = calculateAimError(hitEvents, playableBeatmap);
        }

        private double? calculateAimError(IEnumerable<HitEvent> hitEvents, IBeatmap playableBeatmap)
        {
            IEnumerable<HitEvent> hitCircleEvents = hitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle));

            double nonMissCount = hitCircleEvents.Count(e => e.Result.IsHit());
            double missCount = hitCircleEvents.Count() - nonMissCount;

            if (nonMissCount == 0)
                return null;

            foreach (var e in hitCircleEvents)
            {
                if (e.Position == null)
                    continue;

                hitPoints.Add((e.Position - ((OsuHitObject)e.HitObject).StackedEndPosition).Value);
            }

            double radius = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(playableBeatmap.Difficulty.CircleSize, true);

            // We don't get data for miss locations, so we estimate the total variance using the Rayleigh distribution.
            double variance = (missCount * Math.Pow(radius, 2) + hitPoints.Aggregate(0.0, (current, point) => current + point.LengthSquared)) / (2 * nonMissCount);

            return Math.Sqrt(variance) * 10;
        }

        protected override string DisplayValue(double? value) => value == null ? "(not available)" : value.Value.ToString(@"N2");
    }
}
