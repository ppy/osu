// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
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
            AddStep("Create DHO", () =>
            {
                dho = new TestDrawableHitObject(null);
                dho.Apply(entry = new TestLifetimeEntry(new HitObject())
                {
                    LifetimeStart = 0,
                    LifetimeEnd = 1000,
                });
                Child = dho;
            });

            AddStep("KeepAlive = true", () => entry.KeepAlive = true);
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

        private class TestDrawableHitObject : DrawableHitObject
        {
            public const double INITIAL_LIFETIME_OFFSET = 100;
            protected override double InitialLifetimeOffset => INITIAL_LIFETIME_OFFSET;

            public TestDrawableHitObject(HitObject hitObject)
                : base(hitObject)
            {
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
