// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.NonVisual
{
    [HeadlessTest]
    public class OngoingOperationTrackerTest : OsuTestScene
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
            AddAssert("operation in progress", () => operationInProgress.Value);

            AddStep("cannot start another operation",
                () => Assert.Throws<InvalidOperationException>(() => tracker.BeginOperation()));

            AddStep("end first operation", () => firstOperation.Dispose());
            AddAssert("operation is ended", () => !operationInProgress.Value);

            AddStep("start second operation", () => secondOperation = tracker.BeginOperation());
            AddAssert("operation in progress", () => operationInProgress.Value);

            AddStep("dispose first operation again", () => firstOperation.Dispose());
            AddAssert("operation in progress", () => operationInProgress.Value);

            AddStep("dispose second operation", () => secondOperation.Dispose());
            AddAssert("operation is ended", () => !operationInProgress.Value);
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
    }
}
