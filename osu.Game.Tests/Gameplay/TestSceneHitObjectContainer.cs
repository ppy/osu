// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneHitObjectContainer : OsuTestScene
    {
        private HitObjectContainer container;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = container = new HitObjectContainer();
        });

        [Test]
        public void TestLateHitObjectIsAddedEarlierInList()
        {
            DrawableHitObject hitObject = null;

            AddStep("setup", () => container.Add(new TestDrawableHitObject(new HitObject { StartTime = 500 })));

            AddStep("add late hitobject", () => container.Add(hitObject = new TestDrawableHitObject(new HitObject { StartTime = 1000 })));

            AddAssert("hitobject index is 0", () => container.IndexOf(hitObject) == 0);
        }

        [Test]
        public void TestEarlyHitObjectIsAddedLaterInList()
        {
            DrawableHitObject hitObject = null;

            AddStep("setup", () => container.Add(new TestDrawableHitObject(new HitObject { StartTime = 500 })));

            AddStep("add early hitobject", () => container.Add(hitObject = new TestDrawableHitObject(new HitObject())));

            AddAssert("hitobject index is 0", () => container.IndexOf(hitObject) == 1);
        }

        [Test]
        public void TestHitObjectsResortedAfterStartTimeChange()
        {
            DrawableHitObject firstObject = null;
            DrawableHitObject secondObject = null;

            AddStep("setup", () =>
            {
                container.Add(firstObject = new TestDrawableHitObject(new HitObject()));
                container.Add(secondObject = new TestDrawableHitObject(new HitObject { StartTime = 1000 }));
            });

            AddStep("move first object after second", () => firstObject.HitObject.StartTime = 2000);

            AddAssert("first object index is 1", () => container.IndexOf(firstObject) == 0);
            AddAssert("second object index is 0", () => container.IndexOf(secondObject) == 1);
        }

        private partial class TestDrawableHitObject : DrawableHitObject
        {
            public TestDrawableHitObject([NotNull] HitObject hitObject)
                : base(hitObject)
            {
            }
        }
    }
}
