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
            const int beatLength = 1000;

            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = -beatLength / 3 },
                    new HitObject { StartTime = 0 },
                    new HitObject { StartTime = beatLength / 16 },
                    new HitObject { StartTime = beatLength / 12 },
                    new HitObject { StartTime = beatLength / 8 },
                    new HitObject { StartTime = beatLength / 6 },
                    new HitObject { StartTime = beatLength / 4 },
                    new HitObject { StartTime = beatLength / 3 },
                    new HitObject { StartTime = beatLength / 2 },
                    new HitObject { StartTime = beatLength },
                    new HitObject { StartTime = beatLength + beatLength / 7 }
                },
                ControlPointInfo = new ControlPointInfo()
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint()
                {
                    TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                    BeatLength = beatLength
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
            const int firstBeatLength = 1000;
            const int secondBeatLength = 700;
            const int thirdBeatLength = 200;

            const int firstBeatLengthStart = 0;
            const int secondBeatLengthStart = 1000;
            const int thirdBeatLengthStart = 2000;

            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitObject { StartTime = firstBeatLengthStart },
                    new HitObject { StartTime = firstBeatLengthStart + firstBeatLength / 2 },
                    new HitObject { StartTime = secondBeatLengthStart },
                    new HitObject { StartTime = secondBeatLengthStart + secondBeatLength / 2 },
                    new HitObject { StartTime = thirdBeatLengthStart },
                    new HitObject { StartTime = thirdBeatLengthStart + thirdBeatLength / 2 },
                },
                ControlPointInfo = new ControlPointInfo()
            };

            var firstTimingControlPoint = new TimingControlPoint()
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = firstBeatLength
            };
            
            var secondTimingControlPoint = new TimingControlPoint()
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = secondBeatLength
            };
            
            var thirdTimingControlPoint = new TimingControlPoint()
            {
                TimeSignature = Game.Beatmaps.Timing.TimeSignatures.SimpleQuadruple,
                BeatLength = thirdBeatLength
            };

            beatmap.ControlPointInfo.Add(firstBeatLengthStart, firstTimingControlPoint);
            beatmap.ControlPointInfo.Add(secondBeatLengthStart, secondTimingControlPoint);
            beatmap.ControlPointInfo.Add(thirdBeatLengthStart, thirdTimingControlPoint);

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
