// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    /// <summary>
    /// Utility class to track ongoing online operations' progress.
    /// Can be used to disable interactivity while waiting for a response from online sources.
    /// </summary>
    public class OngoingOperationTracker : Component
    {
        /// <summary>
        /// Whether there is an online operation in progress.
        /// </summary>
        public IBindable<bool> InProgress => inProgress;

        private readonly Bindable<bool> inProgress = new BindableBool();

        private LeasedBindable<bool> leasedInProgress;

        public OngoingOperationTracker()
        {
            AlwaysPresent = true;
        }

        /// <summary>
        /// Begins tracking a new online operation.
        /// </summary>
        /// <returns>
        /// An <see cref="IDisposable"/> that will automatically mark the operation as ended on disposal.
        /// </returns>
        /// <exception cref="InvalidOperationException">An operation has already been started.</exception>
        public IDisposable BeginOperation()
        {
            if (leasedInProgress != null)
                throw new InvalidOperationException("Cannot begin operation while another is in progress.");

            leasedInProgress = inProgress.BeginLease(true);
            leasedInProgress.Value = true;

            return new OngoingOperation(this, leasedInProgress);
        }

        private void endOperationWithKnownLease(LeasedBindable<bool> lease)
        {
            if (lease != leasedInProgress)
                return;

            // for extra safety, marshal the end of operation back to the update thread if necessary.
            Scheduler.Add(() =>
            {
                leasedInProgress?.Return();
                leasedInProgress = null;
            }, false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // base call does an UnbindAllBindables().
            // clean up the leased reference here so that it doesn't get returned twice.
            leasedInProgress = null;
        }

        private class OngoingOperation : IDisposable
        {
            private readonly OngoingOperationTracker tracker;
            private readonly LeasedBindable<bool> lease;

            public OngoingOperation(OngoingOperationTracker tracker, LeasedBindable<bool> lease)
            {
                this.tracker = tracker;
                this.lease = lease;
            }

            public void Dispose()
            {
                tracker.endOperationWithKnownLease(lease);
            }
        }
    }
}
