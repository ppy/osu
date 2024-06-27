// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class TestSceneEditorBeatmapProcessor
    {
        [Test]
        public void TestEmptyBeatmap()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestSingleObjectBeatmap()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestTwoObjectsCloseTogether()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 2000 },
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestHoldNote()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HoldNote { StartTime = 1000, Duration = 10000 },
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new ManiaRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestHoldNoteWithOverlappingNote()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HoldNote { StartTime = 1000, Duration = 10000 },
                    new Note { StartTime = 2000 },
                    new Note { StartTime = 12000 },
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new ManiaRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestTwoObjectsFarApart()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 5000 },
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(1));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1200));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(4000));
            });
        }

        [Test]
        public void TestBreaksAreFused()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 9000 },
                },
                Breaks =
                {
                    new BreakPeriod(1200, 4000),
                    new BreakPeriod(5200, 8000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(1));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1200));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(8000));
            });
        }

        [Test]
        public void TestBreaksAreSplit()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 5000 },
                    new HitCircle { StartTime = 9000 },
                },
                Breaks =
                {
                    new BreakPeriod(1200, 8000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(2));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1200));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(4000));
                Assert.That(beatmap.Breaks[1].StartTime, Is.EqualTo(5200));
                Assert.That(beatmap.Breaks[1].EndTime, Is.EqualTo(8000));
            });
        }

        [Test]
        public void TestBreaksAreNudged()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1100 },
                    new HitCircle { StartTime = 9000 },
                },
                Breaks =
                {
                    new BreakPeriod(1200, 8000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(1));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1300));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(8000));
            });
        }

        [Test]
        public void TestManualBreaksAreNotFused()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 9000 },
                },
                Breaks =
                {
                    new ManualBreakPeriod(1200, 4000),
                    new ManualBreakPeriod(5200, 8000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(2));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1200));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(4000));
                Assert.That(beatmap.Breaks[1].StartTime, Is.EqualTo(5200));
                Assert.That(beatmap.Breaks[1].EndTime, Is.EqualTo(8000));
            });
        }

        [Test]
        public void TestManualBreaksAreSplit()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 5000 },
                    new HitCircle { StartTime = 9000 },
                },
                Breaks =
                {
                    new ManualBreakPeriod(1200, 8000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(2));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1200));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(4000));
                Assert.That(beatmap.Breaks[1].StartTime, Is.EqualTo(5200));
                Assert.That(beatmap.Breaks[1].EndTime, Is.EqualTo(8000));
            });
        }

        [Test]
        public void TestManualBreaksAreNotNudged()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 9000 },
                },
                Breaks =
                {
                    new ManualBreakPeriod(1200, 8800),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.Breaks, Has.Count.EqualTo(1));
                Assert.That(beatmap.Breaks[0].StartTime, Is.EqualTo(1200));
                Assert.That(beatmap.Breaks[0].EndTime, Is.EqualTo(8800));
            });
        }

        [Test]
        public void TestBreaksAtEndOfBeatmapAreRemoved()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 2000 },
                },
                Breaks =
                {
                    new BreakPeriod(10000, 15000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestManualBreaksAtEndOfBeatmapAreRemoved()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 2000 },
                },
                Breaks =
                {
                    new ManualBreakPeriod(10000, 15000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestManualBreaksAtEndOfBeatmapAreRemovedCorrectlyEvenWithConcurrentObjects()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HoldNote { StartTime = 1000, EndTime = 20000 },
                    new HoldNote { StartTime = 2000, EndTime = 3000 },
                },
                Breaks =
                {
                    new ManualBreakPeriod(10000, 15000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestBreaksAtStartOfBeatmapAreRemoved()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 10000 },
                    new HitCircle { StartTime = 11000 },
                },
                Breaks =
                {
                    new BreakPeriod(0, 9000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }

        [Test]
        public void TestManualBreaksAtStartOfBeatmapAreRemoved()
        {
            var controlPoints = new ControlPointInfo();
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 500 });
            var beatmap = new Beatmap
            {
                ControlPointInfo = controlPoints,
                HitObjects =
                {
                    new HitCircle { StartTime = 10000 },
                    new HitCircle { StartTime = 11000 },
                },
                Breaks =
                {
                    new ManualBreakPeriod(0, 9000),
                }
            };

            var beatmapProcessor = new EditorBeatmapProcessor(beatmap, new OsuRuleset());
            beatmapProcessor.PreProcess();
            beatmapProcessor.PostProcess();

            Assert.That(beatmap.Breaks, Is.Empty);
        }
    }
}
