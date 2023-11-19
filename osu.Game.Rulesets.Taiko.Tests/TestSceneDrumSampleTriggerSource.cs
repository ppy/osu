// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneDrumSampleTriggerSource : OsuTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 200 },
        };

        private ScrollingHitObjectContainer hitObjectContainer = null!;
        private TestDrumSampleTriggerSource triggerSource = null!;
        private readonly ManualClock manualClock = new TestManualClock();
        private GameplayClockContainer gameplayClock = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            gameplayClock = new GameplayClockContainer(manualClock, false, false)
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    hitObjectContainer = new ScrollingHitObjectContainer(),
                    triggerSource = new TestDrumSampleTriggerSource(hitObjectContainer)
                }
            };
            gameplayClock.Reset(0);

            hitObjectContainer.Clock = gameplayClock;
            Child = gameplayClock;
        });

        [Test]
        public void TestNormalHit()
        {
            AddStep("add hit with normal samples", () =>
            {
                var hit = new Hit
                {
                    StartTime = 100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                };
                hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableHit = new DrawableHit(hit);
                hitObjectContainer.Add(drawableHit);
            });

            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            seekTo(200);
            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);
        }

        [Test]
        public void TestSoftHit()
        {
            AddStep("add hit with soft samples", () =>
            {
                var hit = new Hit
                {
                    StartTime = 100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT)
                    }
                };
                hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableHit = new DrawableHit(hit);
                hitObjectContainer.Add(drawableHit);
            });

            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            seekTo(200);
            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);
        }

        [Test]
        public void TestBetweenHits()
        {
            Hit first = null!, second = null!;

            AddStep("add hit with normal samples", () =>
            {
                first = new Hit
                {
                    StartTime = 100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                };
                first.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableHit = new DrawableHit(first);
                hitObjectContainer.Add(drawableHit);
            });
            AddStep("add hit with soft samples", () =>
            {
                second = new Hit
                {
                    StartTime = 500,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT),
                        new HitSampleInfo(HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT)
                    }
                };
                second.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableHit = new DrawableHit(second);
                hitObjectContainer.Add(drawableHit);
            });

            AddAssert("most valid object is first hit", () => triggerSource.GetMostValidObject(), () => Is.EqualTo(first));
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_NORMAL);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_NORMAL);

            seekTo(120);
            AddAssert("most valid object is first hit", () => triggerSource.GetMostValidObject(), () => Is.EqualTo(first));
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_NORMAL);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_NORMAL);

            seekTo(480);
            AddAssert("most valid object is second hit", () => triggerSource.GetMostValidObject(), () => Is.EqualTo(second));
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            seekTo(700);
            AddAssert("most valid object is second hit", () => triggerSource.GetMostValidObject(), () => Is.EqualTo(second));
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);
        }

        [Test]
        public void TestDrumStrongHit()
        {
            AddStep("add strong hit with drum samples", () =>
            {
                var hit = new Hit
                {
                    StartTime = 100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_DRUM),
                        new HitSampleInfo(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_DRUM) // implies strong
                    }
                };
                hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableHit = new DrawableHit(hit);
                hitObjectContainer.Add(drawableHit);
            });

            AddAssert("most valid object is nested strong hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit.StrongNestedHit>);
            checkSamples(HitType.Centre, true, $"{HitSampleInfo.HIT_NORMAL},{HitSampleInfo.HIT_FINISH}", HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, true, $"{HitSampleInfo.HIT_CLAP},{HitSampleInfo.HIT_WHISTLE}", HitSampleInfo.BANK_DRUM);

            seekTo(200);
            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSamples(HitType.Centre, true, $"{HitSampleInfo.HIT_NORMAL},{HitSampleInfo.HIT_FINISH}", HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, true, $"{HitSampleInfo.HIT_CLAP},{HitSampleInfo.HIT_WHISTLE}", HitSampleInfo.BANK_DRUM);
        }

        [Test]
        public void TestNormalDrumRoll()
        {
            AddStep("add drum roll with normal samples", () =>
            {
                var drumRoll = new DrumRoll
                {
                    StartTime = 100,
                    EndTime = 1100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                };
                drumRoll.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableDrumRoll = new DrawableDrumRoll(drumRoll);
                hitObjectContainer.Add(drawableDrumRoll);
            });

            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            seekTo(600);
            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            seekTo(1200);
            AddAssert("most valid object is drum roll", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRoll>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);
        }

        [Test]
        public void TestSoftDrumRoll()
        {
            AddStep("add drum roll with soft samples", () =>
            {
                var drumRoll = new DrumRoll
                {
                    StartTime = 100,
                    EndTime = 1100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT)
                    }
                };
                drumRoll.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableDrumRoll = new DrawableDrumRoll(drumRoll);
                hitObjectContainer.Add(drawableDrumRoll);
            });

            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            seekTo(600);
            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            seekTo(1200);
            AddAssert("most valid object is drum roll", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRoll>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);
        }

        [Test]
        public void TestDrumStrongDrumRoll()
        {
            AddStep("add strong drum roll with drum samples", () =>
            {
                var drumRoll = new DrumRoll
                {
                    StartTime = 100,
                    EndTime = 1100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_DRUM),
                        new HitSampleInfo(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_DRUM) // implies strong
                    }
                };
                drumRoll.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableDrumRoll = new DrawableDrumRoll(drumRoll);
                hitObjectContainer.Add(drawableDrumRoll);
            });

            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSamples(HitType.Centre, true, $"{HitSampleInfo.HIT_NORMAL},{HitSampleInfo.HIT_FINISH}", HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, true, $"{HitSampleInfo.HIT_CLAP},{HitSampleInfo.HIT_WHISTLE}", HitSampleInfo.BANK_DRUM);

            seekTo(600);
            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSamples(HitType.Centre, true, $"{HitSampleInfo.HIT_NORMAL},{HitSampleInfo.HIT_FINISH}", HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, true, $"{HitSampleInfo.HIT_CLAP},{HitSampleInfo.HIT_WHISTLE}", HitSampleInfo.BANK_DRUM);

            seekTo(1200);
            AddAssert("most valid object is drum roll", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRoll>);
            checkSamples(HitType.Centre, true, $"{HitSampleInfo.HIT_NORMAL},{HitSampleInfo.HIT_FINISH}", HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, true, $"{HitSampleInfo.HIT_CLAP},{HitSampleInfo.HIT_WHISTLE}", HitSampleInfo.BANK_DRUM);
        }

        [Test]
        public void TestNormalSwell()
        {
            AddStep("add swell with normal samples", () =>
            {
                var swell = new Swell
                {
                    StartTime = 100,
                    EndTime = 1100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                };
                swell.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableSwell = new DrawableSwell(swell);
                hitObjectContainer.Add(drawableSwell);
            });

            // You might think that this should be a SwellTick since we're before the swell, but SwellTicks get no StartTime (ie. they are zero).
            // This works fine in gameplay because they are judged whenever the user pressed, rather than being timed hits.
            // But for sample playback purposes they can be ignored as noise.
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            seekTo(600);
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            seekTo(1200);
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);
        }

        [Test]
        public void TestDrumSwell()
        {
            AddStep("add swell with drum samples", () =>
            {
                var swell = new Swell
                {
                    StartTime = 100,
                    EndTime = 1100,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_DRUM)
                    }
                };
                swell.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableSwell = new DrawableSwell(swell);
                hitObjectContainer.Add(drawableSwell);
            });

            // You might think that this should be a SwellTick since we're before the swell, but SwellTicks get no StartTime (ie. they are zero).
            // This works fine in gameplay because they are judged whenever the user pressed, rather than being timed hits.
            // But for sample playback purposes they can be ignored as noise.
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM);

            seekTo(600);
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM);

            seekTo(1200);
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSamples(HitType.Centre, false, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_DRUM);
            checkSamples(HitType.Rim, false, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM);
        }

        private void checkSamples(HitType hitType, bool strong, string expectedSamplesCsv, string expectedBank)
        {
            AddStep($"hit {hitType}", () => triggerSource.Play(hitType, strong));
            AddAssert($"last played sample is {expectedSamplesCsv}", () => string.Join(',', triggerSource.LastPlayedSamples!.OfType<HitSampleInfo>().Select(s => s.Name)),
                () => Is.EqualTo(expectedSamplesCsv));
            AddAssert($"last played sample has {expectedBank} bank", () => triggerSource.LastPlayedSamples!.OfType<HitSampleInfo>().First().Bank, () => Is.EqualTo(expectedBank));
        }

        private void seekTo(double time) => AddStep($"seek to {time}", () => gameplayClock.Seek(time));

        private partial class TestDrumSampleTriggerSource : DrumSampleTriggerSource
        {
            public ISampleInfo[]? LastPlayedSamples { get; private set; }

            public TestDrumSampleTriggerSource(HitObjectContainer hitObjectContainer)
                : base(hitObjectContainer)
            {
            }

            protected override void PlaySamples(ISampleInfo[] samples)
            {
                base.PlaySamples(samples);
                LastPlayedSamples = samples;
            }

            public new HitObject? GetMostValidObject() => base.GetMostValidObject();
        }

        private class TestManualClock : ManualClock, IAdjustableClock
        {
            public TestManualClock()
            {
                IsRunning = true;
            }

            public void Start() => IsRunning = true;

            public void Stop() => IsRunning = false;

            public bool Seek(double position)
            {
                CurrentTime = position;
                return true;
            }

            public void Reset()
            {
            }

            public void ResetSpeedAdjustments()
            {
            }
        }
    }
}
