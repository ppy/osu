// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Utils
{
    public class SnapFinder
    {
        private ManiaBeatmap beatmap;
        public SnapFinder(ManiaBeatmap beatmap)
        {
            this.beatmap = beatmap;
        }

        public int FindSnap(HitObject hitObject)
        {
            TimingControlPoint currentTimingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);
            int timeSignature = (int)currentTimingPoint.TimeSignature;
            double startTime = currentTimingPoint.Time;
            double secondsPerFourCounts = currentTimingPoint.BeatLength * 4;

            double offset = startTime % secondsPerFourCounts;
            double snapResult = hitObject.StartTime % secondsPerFourCounts - offset;

            if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 4.0))
            {
                return 1;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 8.0))
            {
                return 2;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 12.0))
            {
                return 3;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 16.0))
            {
                return 4;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 24.0))
            {
                return 6;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 32.0))
            {
                return 8;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 48.0))
            {
                return 12;
            }
            else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 64.0))
            {
                return 16;
            }
            else
            {
                return 0;
            }
        }

        private const double LENIENCY_MS = 1.0;
        private static bool AlmostDivisibleBy(double dividend, double divisor)
        {
            double remainder = Math.Abs(dividend) % divisor;
            return Precision.AlmostEquals(remainder, 0, LENIENCY_MS) || Precision.AlmostEquals(remainder - divisor, 0, LENIENCY_MS);
        }
    }
}
