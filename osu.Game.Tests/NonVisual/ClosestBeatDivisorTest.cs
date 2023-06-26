// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.NonVisual
{
    public class ClosestBeatDivisorTest
    {
        [Test]
        public void TestExactDivisors()
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });

            double[] divisors = { 3, 1, 16, 12, 8, 6, 4, 3, 2, 1 };

            assertClosestDivisors(divisors, divisors, cpi);
        }

        [Test]
        public void TestExactDivisorWithTempoChanges()
        {
            int offset = 0;
            int[] beatLengths = { 1000, 200, 100, 50 };

            var cpi = new ControlPointInfo();

            foreach (int beatLength in beatLengths)
            {
                cpi.Add(offset, new TimingControlPoint { BeatLength = beatLength });
                offset += beatLength * 2;
            }

            double[] divisors = { 3, 1, 16, 12, 8, 6, 4, 3 };

            assertClosestDivisors(divisors, divisors, cpi);
        }

        [Test]
        public void TestExactDivisorsHighBPMStream()
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 50 }); // 1200 BPM 1/4 (limit testing)

            // A 1/4 stream should land on 1/1, 1/2 and 1/4 divisors.
            double[] divisors = { 4, 4, 4, 4, 4, 4, 4, 4 };
            double[] closestDivisors = { 4, 2, 4, 1, 4, 2, 4, 1 };

            assertClosestDivisors(divisors, closestDivisors, cpi, step: 1 / 4d);
        }

        [Test]
        public void TestApproximateDivisors()
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });

            double[] divisors = { 3.03d, 0.97d, 14, 13, 7.94d, 6.08d, 3.93d, 2.96d, 2.02d, 64 };
            double[] closestDivisors = { 3, 1, 16, 12, 8, 6, 4, 3, 2, 1 };

            assertClosestDivisors(divisors, closestDivisors, cpi);
        }

        private static void assertClosestDivisors(IReadOnlyList<double> divisors, IReadOnlyList<double> closestDivisors, ControlPointInfo cpi, double step = 1)
        {
            List<HitObject> hitobjects = new List<HitObject>();
            double offset = cpi.TimingPoints[0].Time;

            for (int i = 0; i < divisors.Count; ++i)
            {
                double beatLength = cpi.TimingPointAt(offset).BeatLength;
                hitobjects.Add(new HitObject { StartTime = offset + beatLength / divisors[i] });
                offset += beatLength * step;
            }

            var beatmap = new Beatmap
            {
                HitObjects = hitobjects,
                ControlPointInfo = cpi
            };

            for (int i = 0; i < divisors.Count; ++i)
                Assert.AreEqual(closestDivisors[i], beatmap.ControlPointInfo.GetClosestBeatDivisor(beatmap.HitObjects[i].StartTime), $"at index {i}");
        }
    }
}
