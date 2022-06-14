// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.NonVisual
{
    public class BarLineGeneratorTest
    {
        [Test]
        public void TestRoundingErrorCompensation()
        {
            // The aim of this test is to make sure bar line generation compensates for floating-point errors.
            // The premise of the test is that we have a single timing point that should result in bar lines
            // that start at a time point that is a whole number every seventh beat.

            // The fact it's every seventh beat is important - it's a number indivisible by 2, which makes
            // it susceptible to rounding inaccuracies. In fact this was originally spotted in cases of maps
            // that met exactly this criteria.

            const int beat_length_numerator = 2000;
            const int beat_length_denominator = 7;
            TimeSignature signature = TimeSignature.SimpleQuadruple;

            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = 0 },
                    new HitObject { StartTime = 120_000 }
                },
                ControlPointInfo = new ControlPointInfo()
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint
            {
                BeatLength = (double)beat_length_numerator / beat_length_denominator,
                TimeSignature = signature
            });

            var barLines = new BarLineGenerator<BarLine>(beatmap).BarLines;

            for (int i = 0; i * beat_length_denominator < barLines.Count; i++)
            {
                var barLine = barLines[i * beat_length_denominator];
                int expectedTime = beat_length_numerator * signature.Numerator * i;

                // every seventh bar's start time should be at least greater than the whole number we expect.
                // It cannot be less, as that can affect overlapping scroll algorithms
                // (the previous timing point might be chosen incorrectly if this is not the case)
                Assert.GreaterOrEqual(barLine.StartTime, expectedTime);

                // on the other side, make sure we don't stray too far from the expected time either.
                Assert.IsTrue(Precision.AlmostEquals(barLine.StartTime, expectedTime));

                // check major/minor lines for good measure too
                Assert.AreEqual(i % signature.Numerator == 0, barLine.Major);
            }
        }

        private class BarLine : IBarLine
        {
            public double StartTime { get; set; }
            public bool Major { get; set; }
        }
    }
}
