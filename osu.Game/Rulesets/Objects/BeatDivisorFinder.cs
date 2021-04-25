// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Used to find the lowest beat divisor that a <see cref="HitObject"/> aligns to in an <see cref="IBeatmap"/>.
    /// </summary>
    public class BeatDivisorFinder
    {
        private readonly IBeatmap beatmap;

        /// <summary>
        /// Creates a new <see cref="BeatDivisorFinder"/> instance.
        /// </summary>
        /// <param name="beatmap">The beatmap to use when calculating beat divisor alignment.</param>
        public BeatDivisorFinder(IBeatmap beatmap)
        {
            this.beatmap = beatmap;
        }

        /// <summary>
        /// Finds the lowest beat divisor that the given <see cref="HitObject"/> aligns to.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to evaluate.</param>
        public int FindDivisor(HitObject hitObject)
        {
            TimingControlPoint currentTimingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);
            double snapResult = (hitObject.StartTime - currentTimingPoint.Time) % (currentTimingPoint.BeatLength * 4);

            foreach (var divisor in BindableBeatDivisor.VALID_DIVISORS)
            {
                if (almostDivisibleBy(snapResult, currentTimingPoint.BeatLength / divisor))
                    return divisor;
            }

            return 0;
        }

        private const double leniency_ms = 1.0;

        private static bool almostDivisibleBy(double dividend, double divisor)
        {
            double remainder = Math.Abs(dividend) % divisor;
            return Precision.AlmostEquals(remainder, 0, leniency_ms) || Precision.AlmostEquals(remainder - divisor, 0, leniency_ms);
        }
    }
}
