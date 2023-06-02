// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneDrumSampleTriggerSource : OsuTestScene
    {
        private readonly ManualClock manualClock = new ManualClock();

        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 200 },
        };

        private ScrollingHitObjectContainer hitObjectContainer = null!;
        private TestDrumSampleTriggerSource triggerSource = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            hitObjectContainer = new ScrollingHitObjectContainer();
            manualClock.CurrentTime = 0;

            Child = new Container
            {
                Clock = new FramedClock(manualClock),
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    hitObjectContainer,
                    triggerSource = new TestDrumSampleTriggerSource(hitObjectContainer)
                }
            };
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
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            AddStep("seek past hit", () => manualClock.CurrentTime = 200);
            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);
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
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            AddStep("seek past hit", () => manualClock.CurrentTime = 200);
            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);
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
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, "drum"),
                        new HitSampleInfo(HitSampleInfo.HIT_FINISH, "drum") // implies strong
                    }
                };
                hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableHit = new DrawableHit(hit);
                hitObjectContainer.Add(drawableHit);
            });

            AddAssert("most valid object is strong nested hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit.StrongNestedHit>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");

            AddStep("seek past hit", () => manualClock.CurrentTime = 200);
            AddAssert("most valid object is hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Hit>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");
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
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            AddStep("seek to middle of drum roll", () => manualClock.CurrentTime = 600);
            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            AddStep("seek past drum roll", () => manualClock.CurrentTime = 1200);
            AddAssert("most valid object is drum roll", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRoll>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);
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
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            AddStep("seek to middle of drum roll", () => manualClock.CurrentTime = 600);
            AddAssert("most valid object is drum roll tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);

            AddStep("seek past drum roll", () => manualClock.CurrentTime = 1200);
            AddAssert("most valid object is drum roll", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRoll>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_SOFT);
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
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, "drum"),
                        new HitSampleInfo(HitSampleInfo.HIT_FINISH, "drum") // implies strong
                    }
                };
                drumRoll.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableDrumRoll = new DrawableDrumRoll(drumRoll);
                hitObjectContainer.Add(drawableDrumRoll);
            });

            AddAssert("most valid object is drum roll tick's nested strong hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick.StrongNestedHit>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");

            AddStep("seek to middle of drum roll", () => manualClock.CurrentTime = 600);
            AddAssert("most valid object is drum roll tick's nested strong hit", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRollTick.StrongNestedHit>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");

            AddStep("seek past drum roll", () => manualClock.CurrentTime = 1200);
            AddAssert("most valid object is drum roll", () => triggerSource.GetMostValidObject(), Is.InstanceOf<DrumRoll>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");
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

            AddAssert("most valid object is swell tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<SwellTick>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            AddStep("seek to middle of swell", () => manualClock.CurrentTime = 600);
            AddAssert("most valid object is swell tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<SwellTick>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);

            AddStep("seek past swell", () => manualClock.CurrentTime = 1200);
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, SampleControlPoint.DEFAULT_BANK);
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, SampleControlPoint.DEFAULT_BANK);
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
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, "drum")
                    }
                };
                swell.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                var drawableSwell = new DrawableSwell(swell);
                hitObjectContainer.Add(drawableSwell);
            });

            AddAssert("most valid object is swell tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<SwellTick>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");

            AddStep("seek to middle of swell", () => manualClock.CurrentTime = 600);
            AddAssert("most valid object is swell tick", () => triggerSource.GetMostValidObject(), Is.InstanceOf<SwellTick>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");

            AddStep("seek past swell", () => manualClock.CurrentTime = 1200);
            AddAssert("most valid object is swell", () => triggerSource.GetMostValidObject(), Is.InstanceOf<Swell>);
            checkSound(HitType.Centre, HitSampleInfo.HIT_NORMAL, "drum");
            checkSound(HitType.Rim, HitSampleInfo.HIT_CLAP, "drum");
        }

        private void checkSound(HitType hitType, string expectedName, string expectedBank)
        {
            AddStep($"hit {hitType}", () => triggerSource.Play(hitType, false));
            AddAssert($"last played sample is {expectedName}", () => triggerSource.LastPlayedSamples!.OfType<HitSampleInfo>().Single().Name, () => Is.EqualTo(expectedName));
            AddAssert($"last played sample has {expectedBank} bank", () => triggerSource.LastPlayedSamples!.OfType<HitSampleInfo>().Single().Bank, () => Is.EqualTo(expectedBank));
        }

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

            public new HitObject GetMostValidObject() => base.GetMostValidObject();
        }
    }
}
