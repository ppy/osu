// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.NonVisual
{
    [HeadlessTest]
    public partial class OngoingOperationTrackerTest : OsuTestScene
    {
        private OngoingOperationTracker tracker;
        private IBindable<bool> operationInProgress;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create tracker", () => Child = tracker = new OngoingOperationTracker());
            AddStep("bind to operation status", () => operationInProgress = tracker.InProgress.GetBoundCopy());
        }

        [Test]
        public void TestOperationTracking()
        {
            IDisposable firstOperation = null;
            IDisposable secondOperation = null;

            AddStep("begin first operation", () => firstOperation = tracker.BeginOperation());
            AddAssert("first operation in progress", () => operationInProgress.Value);

            AddStep("cannot start another operation",
                () => Assert.Throws<InvalidOperationException>(() => tracker.BeginOperation()));

            AddStep("end first operation", () => firstOperation.Dispose());
            AddAssert("first operation is ended", () => !operationInProgress.Value);

            AddStep("start second operation", () => secondOperation = tracker.BeginOperation());
            AddAssert("second operation in progress", () => operationInProgress.Value);

            AddStep("dispose first operation again", () => firstOperation.Dispose());
            AddAssert("second operation still in progress", () => operationInProgress.Value);

            AddStep("dispose second operation", () => secondOperation.Dispose());
            AddAssert("second operation is ended", () => !operationInProgress.Value);
        }

        [Test]
        public void TestOperationDisposalAfterTracker()
        {
            IDisposable operation = null;

            AddStep("begin operation", () => operation = tracker.BeginOperation());
            AddStep("dispose tracker", () => tracker.Expire());
            AddStep("end operation", () => operation.Dispose());
            AddAssert("operation is ended", () => !operationInProgress.Value);
        }

        [Test]
        public void TestOperationDisposalAfterScreenExit()
        {
            TestScreenWithTracker screen = null;
            OsuScreenStack stack;
            IDisposable operation = null;

            AddStep("create screen with tracker", () =>
            {
                Child = stack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both
                };

                stack.Push(screen = new TestScreenWithTracker());
            });
            AddUntilStep("wait for loaded", () => screen.IsLoaded);

            AddStep("begin operation", () => operation = screen.OngoingOperationTracker.BeginOperation());
            AddAssert("operation in progress", () => screen.OngoingOperationTracker.InProgress.Value);

            AddStep("dispose after screen exit", () =>
            {
                screen.Exit();
                operation.Dispose();
            });
            AddAssert("operation ended", () => !screen.OngoingOperationTracker.InProgress.Value);
        }

        private partial class TestScreenWithTracker : OsuScreen
        {
            public OngoingOperationTracker OngoingOperationTracker { get; private set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = OngoingOperationTracker = new OngoingOperationTracker();
            }
        }
    }
}
