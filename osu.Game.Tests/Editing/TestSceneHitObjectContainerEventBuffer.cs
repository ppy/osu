// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Editing
{
    [HeadlessTest]
    public partial class TestSceneHitObjectContainerEventBuffer : OsuTestScene
    {
        private readonly TestHitObject testObj = new TestHitObject();

        private TestPlayfield playfield1;
        private TestPlayfield playfield2;
        private TestDrawable intermediateDrawable;
        private HitObjectUsageEventBuffer eventBuffer;

        private HitObject beganUsage;
        private HitObject finishedUsage;
        private HitObject transferredUsage;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            reset();

            if (eventBuffer != null)
            {
                eventBuffer.HitObjectUsageBegan -= onHitObjectUsageBegan;
                eventBuffer.HitObjectUsageFinished -= onHitObjectUsageFinished;
                eventBuffer.HitObjectUsageTransferred -= onHitObjectUsageTransferred;
            }

            var topPlayfield = new TestPlayfield();
            topPlayfield.AddNested(playfield1 = new TestPlayfield());
            topPlayfield.AddNested(playfield2 = new TestPlayfield());

            eventBuffer = new HitObjectUsageEventBuffer(topPlayfield);
            eventBuffer.HitObjectUsageBegan += onHitObjectUsageBegan;
            eventBuffer.HitObjectUsageFinished += onHitObjectUsageFinished;
            eventBuffer.HitObjectUsageTransferred += onHitObjectUsageTransferred;

            Children = new Drawable[]
            {
                topPlayfield,
                intermediateDrawable = new TestDrawable(),
            };
        });

        private void onHitObjectUsageBegan(HitObject obj) => beganUsage = obj;

        private void onHitObjectUsageFinished(HitObject obj) => finishedUsage = obj;

        private void onHitObjectUsageTransferred(HitObject obj, DrawableHitObject drawableObj) => transferredUsage = obj;

        [Test]
        public void TestUsageBeganAfterAdd()
        {
            AddStep("add hitobject", () => playfield1.Add(testObj));
            addCheckStep(began: true);
        }

        [Test]
        public void TestUsageFinishedAfterRemove()
        {
            AddStep("add hitobject", () => playfield1.Add(testObj));
            addResetStep();
            AddStep("remove hitobject", () => playfield1.Remove(testObj));
            addCheckStep(finished: true);
        }

        [Test]
        public void TestUsageTransferredWhenMovedBetweenPlayfields()
        {
            AddStep("add hitobject", () => playfield1.Add(testObj));
            addResetStep();
            AddStep("transfer hitobject to other playfield", () =>
            {
                playfield1.Remove(testObj);
                playfield2.Add(testObj);
            });

            addCheckStep(transferred: true);
        }

        [Test]
        public void TestRemoveImmediatelyAfterUsageBegan()
        {
            AddStep("add hitobject and schedule removal", () =>
            {
                playfield1.Add(testObj);
                intermediateDrawable.Schedule(() => playfield1.Remove(testObj));
            });

            addCheckStep(began: true, finished: true);
        }

        [Test]
        public void TestRemoveImmediatelyAfterTransferred()
        {
            AddStep("add hitobject", () => playfield1.Add(testObj));
            addResetStep();
            AddStep("transfer hitobject to other playfield and schedule removal", () =>
            {
                playfield1.Remove(testObj);
                playfield2.Add(testObj);
                intermediateDrawable.Schedule(() => playfield2.Remove(testObj));
            });

            addCheckStep(transferred: true, finished: true);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            eventBuffer.Update();
        }

        private void addResetStep() => AddStep("reset", reset);

        private void reset()
        {
            beganUsage = null;
            finishedUsage = null;
            transferredUsage = null;
        }

        private void addCheckStep(bool began = false, bool finished = false, bool transferred = false)
            => AddAssert($"began = {began}, finished = {finished}, transferred = {transferred}",
                () => (beganUsage == testObj) == began && (finishedUsage == testObj) == finished && (transferredUsage == testObj) == transferred);

        private partial class TestPlayfield : Playfield
        {
            public TestPlayfield()
            {
                RegisterPool<TestHitObject, TestDrawableHitObject>(1);
            }

            public new void AddNested(Playfield playfield)
            {
                AddInternal(playfield);
                base.AddNested(playfield);
            }

            protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject)
            {
                var entry = base.CreateLifetimeEntry(hitObject);
                entry.KeepAlive = true;
                return entry;
            }
        }

        private class TestHitObject : HitObject
        {
            public override string ToString() => "TestHitObject";
        }

        private partial class TestDrawableHitObject : DrawableHitObject
        {
        }

        private partial class TestDrawable : Drawable
        {
            public new void Schedule(Action action) => base.Schedule(action);
        }
    }
}
