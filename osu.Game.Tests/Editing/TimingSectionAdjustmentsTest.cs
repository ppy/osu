// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class TimingSectionAdjustmentsTest
    {
        [Test]
        public void TestOffsetAdjustment()
        {
            var controlPoints = new ControlPointInfo();

            controlPoints.Add(100, new TimingControlPoint { BeatLength = 100 });
            controlPoints.Add(50_000, new TimingControlPoint { BeatLength = 200 });
            controlPoints.Add(100_000, new TimingControlPoint { BeatLength = 50 });

            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 200 },
                    new HitCircle { StartTime = 49_900 },
                    new HitCircle { StartTime = 50_000 },
                    new HitCircle { StartTime = 50_200 },
                    new HitCircle { StartTime = 99_800 },
                    new HitCircle { StartTime = 100_000 },
                    new HitCircle { StartTime = 100_050 },
                    new HitCircle { StartTime = 100_550 },
                }
            };

            moveTimingPoint(beatmap, 100, -50);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.HitObjects[0].StartTime, Is.EqualTo(-50));
                Assert.That(beatmap.HitObjects[1].StartTime, Is.EqualTo(150));
                Assert.That(beatmap.HitObjects[2].StartTime, Is.EqualTo(49_850));
                Assert.That(beatmap.HitObjects[3].StartTime, Is.EqualTo(50_000));
            });

            moveTimingPoint(beatmap, 50_000, 1_000);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.HitObjects[2].StartTime, Is.EqualTo(49_850));
                Assert.That(beatmap.HitObjects[3].StartTime, Is.EqualTo(51_000));
                Assert.That(beatmap.HitObjects[4].StartTime, Is.EqualTo(51_200));
                Assert.That(beatmap.HitObjects[5].StartTime, Is.EqualTo(100_800));
                Assert.That(beatmap.HitObjects[6].StartTime, Is.EqualTo(100_000));
            });

            moveTimingPoint(beatmap, 100_000, 10_000);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.HitObjects[4].StartTime, Is.EqualTo(51_200));
                Assert.That(beatmap.HitObjects[5].StartTime, Is.EqualTo(110_800));
                Assert.That(beatmap.HitObjects[6].StartTime, Is.EqualTo(110_000));
                Assert.That(beatmap.HitObjects[7].StartTime, Is.EqualTo(110_050));
                Assert.That(beatmap.HitObjects[8].StartTime, Is.EqualTo(110_550));
            });
        }

        [Test]
        public void TestBPMAdjustment()
        {
            var controlPoints = new ControlPointInfo();

            controlPoints.Add(100, new TimingControlPoint { BeatLength = 100 });
            controlPoints.Add(50_000, new TimingControlPoint { BeatLength = 200 });
            controlPoints.Add(100_000, new TimingControlPoint { BeatLength = 50 });

            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 200 },
                    new Spinner { StartTime = 500, EndTime = 1000 },
                    new HitCircle { StartTime = 49_900 },
                    new HitCircle { StartTime = 50_000 },
                    new HitCircle { StartTime = 50_200 },
                    new HitCircle { StartTime = 99_800 },
                    new HitCircle { StartTime = 100_000 },
                    new HitCircle { StartTime = 100_050 },
                    new HitCircle { StartTime = 100_550 },
                }
            };

            adjustBeatLength(beatmap, 100, 50);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.HitObjects[0].StartTime, Is.EqualTo(50));
                Assert.That(beatmap.HitObjects[1].StartTime, Is.EqualTo(150));
                Assert.That(beatmap.HitObjects[2].StartTime, Is.EqualTo(300));
                Assert.That(beatmap.HitObjects[2].GetEndTime(), Is.EqualTo(550));
                Assert.That(beatmap.HitObjects[3].StartTime, Is.EqualTo(25_000));
                Assert.That(beatmap.HitObjects[4].StartTime, Is.EqualTo(50_000));
            });

            adjustBeatLength(beatmap, 50_000, 400);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.HitObjects[2].StartTime, Is.EqualTo(300));
                Assert.That(beatmap.HitObjects[2].GetEndTime(), Is.EqualTo(550));
                Assert.That(beatmap.HitObjects[3].StartTime, Is.EqualTo(25_000));
                Assert.That(beatmap.HitObjects[4].StartTime, Is.EqualTo(50_000));
                Assert.That(beatmap.HitObjects[5].StartTime, Is.EqualTo(50_400));
                Assert.That(beatmap.HitObjects[6].StartTime, Is.EqualTo(149_600));
                Assert.That(beatmap.HitObjects[7].StartTime, Is.EqualTo(100_000));
            });

            adjustBeatLength(beatmap, 100_000, 100);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.HitObjects[5].StartTime, Is.EqualTo(50_400));
                Assert.That(beatmap.HitObjects[6].StartTime, Is.EqualTo(199_200));
                Assert.That(beatmap.HitObjects[7].StartTime, Is.EqualTo(100_000));
                Assert.That(beatmap.HitObjects[8].StartTime, Is.EqualTo(100_100));
                Assert.That(beatmap.HitObjects[9].StartTime, Is.EqualTo(101_100));
            });
        }

        private static void moveTimingPoint(IBeatmap beatmap, double originalTime, double adjustment)
        {
            var controlPoints = beatmap.ControlPointInfo;
            var controlPointGroup = controlPoints.GroupAt(originalTime);
            var timingPoint = controlPointGroup.ControlPoints.OfType<TimingControlPoint>().Single();
            controlPoints.RemoveGroup(controlPointGroup);
            TimingSectionAdjustments.AdjustHitObjectOffset(beatmap, timingPoint, adjustment);
            controlPoints.Add(originalTime - adjustment, timingPoint);
        }

        private static void adjustBeatLength(IBeatmap beatmap, double groupTime, double newBeatLength)
        {
            var controlPoints = beatmap.ControlPointInfo;
            var controlPointGroup = controlPoints.GroupAt(groupTime);
            var timingPoint = controlPointGroup.ControlPoints.OfType<TimingControlPoint>().Single();
            double oldBeatLength = timingPoint.BeatLength;
            timingPoint.BeatLength = newBeatLength;
            TimingSectionAdjustments.SetHitObjectBPM(beatmap, timingPoint, oldBeatLength);
        }
    }
}
