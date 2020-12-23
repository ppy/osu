// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneCaughtObjectContainer : OsuTestScene
    {
        private StopwatchClock clock;
        private Container<DrawableCaughtObject> stackedObjectContainer;
        private Container<DrawableCaughtObject> droppedObjectContainer;
        private CaughtObjectContainer caughtObjectContainer;
        private CaughtObjectEntry lastEntry;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("initialize", () =>
            {
                SetUp();
                Schedule(() => clock.Start());
            });
            stackFruit();
            addDroplet();
            dropAll();
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Clock = new FramedClock(clock = new StopwatchClock()),
                Children = new Drawable[]
                {
                    droppedObjectContainer = new Container<DrawableCaughtObject>(),
                    caughtObjectContainer = new CaughtObjectContainer(droppedObjectContainer)
                }
            };
            stackedObjectContainer = caughtObjectContainer.StackedObjectContainer;
        });

        [Test]
        public void TestStackObjectsAreDropped()
        {
            stackFruit();
            checkStackedObjects(1);
            stackFruit();
            checkStackedObjects(2);
            dropAll();
            checkStackedObjects(0);
            checkDroppedObjects(2);
            seekTime(1000);
            checkDroppedObjects(0);
        }

        [Test]
        public void TestStackedObjectLifetime()
        {
            stackFruit();
            seekTime(-1);
            checkStackedObjects(0);
            seekTime(0);
            checkStackedObjects(1);
        }

        [Test]
        public void TestDroppedObjectLifetime()
        {
            addDroplet();
            checkDroppedObjects(1);
            seekTime(-1);
            checkDroppedObjects(0);
            seekTime(0);
            checkDroppedObjects(1);
            seekTime(1000);
            checkDroppedObjects(0);
        }

        [Test]
        public void TestStackedAndDroppedObjectLifetime()
        {
            stackFruit();
            seekTime(1000);
            dropAll();
            seekTime(2000);
            checkDroppedObjects(0);
            seekTime(1000);
            checkDroppedObjects(1);
            seekTime(999);
            checkDroppedObjects(0);
            checkStackedObjects(1);
            seekTime(-1);
            checkStackedObjects(0);
            seekTime(1000);
            checkStackedObjects(0);
            checkDroppedObjects(1);
        }

        [Test]
        public void TestStackedObjectRemovalAlsoRemovesDroppedObject()
        {
            stackFruit();
            seekTime(1000);
            dropAll();
            AddStep("remove stacked object", () => caughtObjectContainer.Remove(lastEntry));
            checkDroppedObjects(0);
            seekTime(0);
            checkStackedObjects(0);
        }

        [Test]
        public void TestOnlyAliveObjectsAreDropped()
        {
            stackFruit();
            stackFruit();
            seekTime(1000);
            stackFruit();
            seekTime(0);
            checkStackedObjects(2);
            dropAll();
            checkDroppedObjects(2);
            seekTime(1000);
            checkStackedObjects(1);
            checkDroppedObjects(0);
        }

        [Test]
        public void TestDropPositionIsCorrect()
        {
            stackFruit();
            AddStep("move stack position", () => caughtObjectContainer.X = 200);
            dropAll();
            AddAssert("object position is correct", () => droppedObjectContainer[0].X == 200);
            seekTime(-1);
            AddStep("move stack position", () => caughtObjectContainer.X = 0);
            seekTime(0);
            AddAssert("object position is correct", () => droppedObjectContainer[0].X == 200);
        }

        [Test]
        public void TestDroppedObjectIsMirrored()
        {
            stackFruit();
            AddStep("mirror stack", () => caughtObjectContainer.Scale *= new Vector2(-1, 1));
            dropAll();
            AddAssert("object is mirrored", () => droppedObjectContainer[0].Scale.X < 0);
            seekTime(-1);
            AddStep("mirror stack", () => caughtObjectContainer.Scale *= new Vector2(-1, 1));
            seekTime(0);
            AddAssert("object is mirrored", () => droppedObjectContainer[0].Scale.X < 0);
        }

        private void stackFruit() => AddStep("stack fruit", () => caughtObjectContainer.Add(lastEntry = createEntry()));

        private void addDroplet() => AddStep("add droplet", () => caughtObjectContainer.Add(lastEntry = createEntry(true)));

        private void dropAll() => AddStep("drop all", () => caughtObjectContainer.DropStackedObjects(applyDropTransforms, Math.Sign(caughtObjectContainer.Scale.X)));

        private void seekTime(double time) => AddStep($"seek time to {time}", () => clock.Seek(time));

        private void checkStackedObjects(int count) =>
            AddAssert($"{count} stacked objects", () =>
            {
                return stackedObjectContainer.Count == count &&
                       stackedObjectContainer.All(d => d.IsPresent);
            });

        private void checkDroppedObjects(int count) =>
            AddAssert($"{count} dropped objects", () =>
            {
                return droppedObjectContainer.Count == count &&
                       droppedObjectContainer.All(d => d.IsPresent);
            });

        private void applyDropTransforms(DrawableCaughtObject d)
        {
            d.FadeTo(0.5f);
            d.FadeOut(750);
        }

        private void applyExplodingTransforms(DrawableCaughtObject d)
        {
            d.FadeTo(0.5f);
            d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then()
             .MoveToY(d.Y + 50, 500, Easing.InSine);
        }

        private CaughtObjectEntry createEntry(bool droplet = false)
        {
            var state = !droplet ? CaughtObjectState.Stacked : CaughtObjectState.Dropped;
            var positionInStack = caughtObjectContainer.GetPositionInStack(Vector2.Zero, 500);
            return new CaughtObjectEntry(state, positionInStack, new TestCatchObjectState
            {
                HitObject = !droplet
                    ? (CatchHitObject)new Fruit
                    {
                        StartTime = clock.CurrentTime
                    }
                    : new Droplet
                    {
                        StartTime = clock.CurrentTime
                    },
            })
            {
                LifetimeStart = clock.CurrentTime,
                ApplyTransforms = !droplet ? (Action<DrawableCaughtObject>)null : applyExplodingTransforms
            };
        }

        private class TestCatchObjectState : IHasCatchObjectState
        {
            public CatchHitObject HitObject { get; set; }
            public Bindable<Color4> AccentColour { get; } = new Bindable<Color4>(Colour4.Green);
            public Bindable<bool> HyperDash { get; } = new Bindable<bool>();
            public Vector2 DisplaySize => new Vector2(500);
            public float DisplayRotation => 100;
        }
    }
}
