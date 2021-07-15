// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneDrawableHitObject : OsuTestScene
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
            AddStep("ApplyDefaults", () => entry.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty()));
            AddAssert("Lifetime is updated", () => entry.LifetimeStart == -TestLifetimeEntry.INITIAL_LIFETIME_OFFSET);

            TestDrawableHitObject dho = null;
            AddStep("Create DHO", () => Child = dho = new TestDrawableHitObject
            {
                Entry = entry,
                SetLifetimeStartOnApply = true
            });
            AddStep("ApplyDefaults", () => entry.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty()));
            AddAssert("Lifetime is correct", () => dho.LifetimeStart == TestDrawableHitObject.LIFETIME_ON_APPLY && entry.LifetimeStart == TestDrawableHitObject.LIFETIME_ON_APPLY);
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

        private class TestDrawableHitObject : DrawableHitObject
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
