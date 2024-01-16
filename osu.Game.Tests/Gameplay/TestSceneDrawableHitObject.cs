// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneDrawableHitObject : OsuTestScene
    {
        [Test]
        public void TestEntryLifetime()
        {
            TestDrawableHitObject dho = null;
            var initialHitObject = new HitObject
            {
                StartTime = 1000
            };
            var entry = new TestLifetimeEntry(new HitObject
            {
                StartTime = 2000
            });

            AddStep("Create DHO", () => Child = dho = new TestDrawableHitObject(initialHitObject));

            AddAssert("Correct initial lifetime", () => dho.LifetimeStart == initialHitObject.StartTime - TestDrawableHitObject.INITIAL_LIFETIME_OFFSET);

            AddStep("Apply entry", () => dho.Apply(entry));

            AddAssert("Correct initial lifetime", () => dho.LifetimeStart == entry.HitObject.StartTime - TestLifetimeEntry.INITIAL_LIFETIME_OFFSET);

            AddStep("Set lifetime", () => dho.LifetimeEnd = 3000);
            AddAssert("Entry lifetime is updated", () => entry.LifetimeEnd == 3000);
        }

        [Test]
        public void TestKeepAlive()
        {
            TestDrawableHitObject dho = null;
            TestLifetimeEntry entry = null;
            AddStep("Create DHO", () => Child = dho = new TestDrawableHitObject
            {
                Entry = entry = new TestLifetimeEntry(new HitObject())
            });

            AddStep("KeepAlive = true", () =>
            {
                entry.LifetimeStart = 0;
                entry.LifetimeEnd = 1000;
                entry.KeepAlive = true;
            });
            AddAssert("Lifetime is overriden", () => entry.LifetimeStart == double.MinValue && entry.LifetimeEnd == double.MaxValue);

            AddStep("Set LifetimeStart", () => dho.LifetimeStart = 500);
            AddStep("KeepAlive = false", () => entry.KeepAlive = false);
            AddAssert("Lifetime is correct", () => entry.LifetimeStart == 500 && entry.LifetimeEnd == 1000);

            AddStep("Set LifetimeStart while KeepAlive", () =>
            {
                entry.KeepAlive = true;
                dho.LifetimeStart = double.MinValue;
                entry.KeepAlive = false;
            });
            AddAssert("Lifetime is changed", () => entry.LifetimeStart == double.MinValue && entry.LifetimeEnd == 1000);
        }

        [Test]
        public void TestLifetimeUpdatedOnDefaultApplied()
        {
            TestLifetimeEntry entry = null;
            AddStep("Create entry", () => entry = new TestLifetimeEntry(new HitObject()) { LifetimeStart = 1 });
            assertJudged(() => entry, false);
            AddStep("ApplyDefaults", () => entry.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty()));
            assertJudged(() => entry, false);
            AddAssert("Lifetime is updated", () => entry.LifetimeStart == -TestLifetimeEntry.INITIAL_LIFETIME_OFFSET);

            TestDrawableHitObject dho = null;
            AddStep("Create DHO", () => Child = dho = new TestDrawableHitObject
            {
                Entry = entry,
                SetLifetimeStartOnApply = true
            });
            AddStep("ApplyDefaults", () => entry.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty()));
            AddAssert("Lifetime is correct", () => dho.LifetimeStart == TestDrawableHitObject.LIFETIME_ON_APPLY && entry.LifetimeStart == TestDrawableHitObject.LIFETIME_ON_APPLY);
            assertJudged(() => entry, false);
        }

        [Test]
        public void TestDrawableLifetimeUpdateOnEntryLifetimeChange()
        {
            TestDrawableHitObject dho = null;
            TestLifetimeEntry entry = null;
            AddStep("Create DHO", () => Child = dho = new TestDrawableHitObject
            {
                Entry = entry = new TestLifetimeEntry(new HitObject())
            });

            AddStep("Set entry lifetime", () =>
            {
                entry.LifetimeStart = 777;
                entry.LifetimeEnd = 888;
            });
            AddAssert("Drawable lifetime is updated", () => dho.LifetimeStart == 777 && dho.LifetimeEnd == 888);

            AddStep("KeepAlive = true", () => entry.KeepAlive = true);
            AddAssert("Drawable lifetime is updated", () => dho.LifetimeStart == double.MinValue && dho.LifetimeEnd == double.MaxValue);

            AddStep("Modify start time", () => entry.HitObject.StartTime = 100);
            AddAssert("Drawable lifetime is correct", () => dho.LifetimeStart == double.MinValue);

            AddStep("Set LifetimeStart", () => dho.LifetimeStart = 666);
            AddAssert("Lifetime change is blocked", () => dho.LifetimeStart == double.MinValue);

            AddStep("Set LifetimeEnd", () => dho.LifetimeEnd = 999);
            AddAssert("Lifetime change is blocked", () => dho.LifetimeEnd == double.MaxValue);

            AddStep("KeepAlive = false", () => entry.KeepAlive = false);
            AddAssert("Drawable lifetime is restored", () => dho.LifetimeStart == 666 && dho.LifetimeEnd == 999);
        }

        [Test]
        public void TestStateChangeBeforeLoadComplete()
        {
            TestDrawableHitObject dho = null;
            AddStep("Add DHO and apply result", () =>
            {
                Child = dho = new TestDrawableHitObject(new HitObject { StartTime = Time.Current });
                dho.MissForcefully();
            });
            AddAssert("DHO state is correct", () => dho.State.Value == ArmedState.Miss);
        }

        [Test]
        public void TestJudgedStateThroughLifetime()
        {
            TestDrawableHitObject dho = null;
            HitObjectLifetimeEntry lifetimeEntry = null;

            AddStep("Create lifetime entry", () => lifetimeEntry = new HitObjectLifetimeEntry(new HitObject { StartTime = Time.Current }));

            assertJudged(() => lifetimeEntry, false);

            AddStep("Create DHO and apply entry", () =>
            {
                Child = dho = new TestDrawableHitObject();
                dho.Apply(lifetimeEntry);
            });

            assertJudged(() => lifetimeEntry, false);

            AddStep("Apply result", () => dho.MissForcefully());

            assertJudged(() => lifetimeEntry, true);
        }

        [Test]
        public void TestResultSetBeforeLoadComplete()
        {
            TestDrawableHitObject dho = null;
            HitObjectLifetimeEntry lifetimeEntry = null;
            AddStep("Create lifetime entry", () =>
            {
                var hitObject = new HitObject { StartTime = Time.Current };
                lifetimeEntry = new HitObjectLifetimeEntry(hitObject)
                {
                    Result = new Judgement(hitObject, hitObject.CreateJudgement())
                    {
                        Type = HitResult.Great
                    }
                };
            });
            assertJudged(() => lifetimeEntry, true);
            AddStep("Create DHO and apply entry", () =>
            {
                dho = new TestDrawableHitObject();
                dho.Apply(lifetimeEntry);
                Child = dho;
            });
            assertJudged(() => lifetimeEntry, true);
            AddAssert("DHO state is correct", () => dho.State.Value, () => Is.EqualTo(ArmedState.Hit));
        }

        private void assertJudged(Func<HitObjectLifetimeEntry> entry, bool val) =>
            AddAssert(val ? "Is judged" : "Not judged", () => entry().Judged, () => Is.EqualTo(val));

        private partial class TestDrawableHitObject : DrawableHitObject
        {
            public const double INITIAL_LIFETIME_OFFSET = 100;
            public const double LIFETIME_ON_APPLY = 222;
            protected override double InitialLifetimeOffset => INITIAL_LIFETIME_OFFSET;

            public bool SetLifetimeStartOnApply;

            public TestDrawableHitObject(HitObject hitObject = null)
                : base(hitObject)
            {
            }

            protected override void OnApply()
            {
                base.OnApply();

                if (SetLifetimeStartOnApply)
                    LifetimeStart = LIFETIME_ON_APPLY;
            }

            public void MissForcefully() => ApplyResult(r => r.Type = HitResult.Miss);

            protected override void UpdateHitStateTransforms(ArmedState state)
            {
                if (state != ArmedState.Miss)
                {
                    base.UpdateHitStateTransforms(state);
                    return;
                }

                this.FadeOut(1000);
            }
        }

        private class TestLifetimeEntry : HitObjectLifetimeEntry
        {
            public const double INITIAL_LIFETIME_OFFSET = 200;
            protected override double InitialLifetimeOffset => INITIAL_LIFETIME_OFFSET;

            public TestLifetimeEntry(HitObject hitObject)
                : base(hitObject)
            {
            }
        }
    }
}
