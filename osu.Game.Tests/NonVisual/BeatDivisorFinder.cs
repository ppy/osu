// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.NonVisual
{
    public class BeatDivisorFinderTest
    {
        [Test]
        public void TestFindDivisor()
        {
            const double beat_length = 1000;

            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = -beat_length / 3 },
                    new HitObject { StartTime = 0 },
                    new HitObject { StartTime = beat_length / 16 },
                    new HitObject { StartTime = beat_length / 12 },
                    new HitObject { StartTime = beat_length / 8 },
                    new HitObject { StartTime = beat_length / 6 },
                    new HitObject { StartTime = beat_length / 4 },
                    new HitObject { StartTime = beat_length / 3 },
                    new HitObject { StartTime = beat_length / 2 },
                    new HitObject { StartTime = beat_length },
                    new HitObject { StartTime = beat_length + beat_length / 7 }
                },
                ControlPointInfo = new ControlPointInfo()
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = beat_length
            });

            var beatDivisorFinder = new BeatDivisorFinder(beatmap);

            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[0]), 3);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[1]), 1);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[2]), 16);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[3]), 12);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[4]), 8);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[5]), 6);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[6]), 4);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[7]), 3);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[8]), 2);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[9]), 1);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[10]), 0);
        }

        [Test]
        public void TestFindDivisorWithTempoChanges()
        {
            const double first_beat_length = 1000;
            const double second_beat_length = 700;
            const double third_beat_length = 200;

            const double first_beat_length_start = 0;
            const double second_beat_length_start = 1000;
            const double third_beat_length_start = 2000;

            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = first_beat_length_start },
                    new HitObject { StartTime = first_beat_length_start + first_beat_length / 2 },
                    new HitObject { StartTime = second_beat_length_start },
                    new HitObject { StartTime = second_beat_length_start + second_beat_length / 2 },
                    new HitObject { StartTime = third_beat_length_start },
                    new HitObject { StartTime = third_beat_length_start + third_beat_length / 2 },
                },
                ControlPointInfo = new ControlPointInfo()
            };

            var firstTimingControlPoint = new TimingControlPoint
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = first_beat_length
            };

            var secondTimingControlPoint = new TimingControlPoint
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = second_beat_length
            };

            var thirdTimingControlPoint = new TimingControlPoint
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = third_beat_length
            };

            beatmap.ControlPointInfo.Add(first_beat_length_start, firstTimingControlPoint);
            beatmap.ControlPointInfo.Add(second_beat_length_start, secondTimingControlPoint);
            beatmap.ControlPointInfo.Add(third_beat_length_start, thirdTimingControlPoint);

            var beatDivisorFinder = new BeatDivisorFinder(beatmap);

            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[0]), 1);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[1]), 2);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[2]), 1);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[3]), 2);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[4]), 1);
            Assert.AreEqual(beatDivisorFinder.FindDivisor(beatmap.HitObjects[5]), 2);
        }
    }
}
