// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;

namespace osu.Game.Online
{
    /// <summary>
    /// A component which requires a constant polling process.
    /// </summary>
    public abstract class PollingComponent : CompositeDrawable // switch away from Component because InternalChildren are used in usages.
    {
        private double? lastTimePolled;

        private ScheduledDelegate scheduledPoll;

        private bool pollingActive;

        /// <summary>
        /// The time in milliseconds to wait between polls.
        /// Setting to zero stops all polling.
        /// </summary>
        public readonly Bindable<double> TimeBetweenPolls = new Bindable<double>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="timeBetweenPolls">The initial time in milliseconds to wait between polls. Setting to zero stops all polling.</param>
        protected PollingComponent(double timeBetweenPolls = 0)
        {
            TimeBetweenPolls.BindValueChanged(_ =>
            {
                scheduledPoll?.Cancel();
                pollIfNecessary();
            });

            TimeBetweenPolls.Value = timeBetweenPolls;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            pollIfNecessary();
        }

        /// <summary>
        /// Immediately performs a <see cref="Poll"/>.
        /// </summary>
        public void PollImmediately()
        {
            lastTimePolled = Time.Current - TimeBetweenPolls.Value;
            scheduleNextPoll();
        }

        /// <summary>
        /// Performs a poll. Implement but do not call this.
        /// </summary>
        protected virtual Task Poll()
        {
            return Task.CompletedTask;
        }

        private void doPoll()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            scheduledPoll = null;
            pollingActive = true;
            Poll().ContinueWith(_ => pollComplete());
        }

        /// <summary>
        /// Call when a poll operation has completed.
        /// </summary>
        private void pollComplete()
        {
            lastTimePolled = Time.Current;
            pollingActive = false;

            if (scheduledPoll == null)
                pollIfNecessary();
        }

        private void pollIfNecessary()
        {
            // we must be loaded so we have access to clock.
            if (!IsLoaded) return;

            // there's already a poll process running.
            if (pollingActive) return;

            // don't try polling if the time between polls hasn't been set.
            if (TimeBetweenPolls.Value == 0) return;

            if (!lastTimePolled.HasValue)
            {
                Scheduler.AddOnce(doPoll);
                return;
            }

            if (Time.Current - lastTimePolled.Value > TimeBetweenPolls.Value)
            {
                Scheduler.AddOnce(doPoll);
                return;
            }

            // not enough time has passed since the last poll. we do want to schedule a poll to happen, though.
            scheduleNextPoll();
        }

        private void scheduleNextPoll()
        {
            scheduledPoll?.Cancel();

            double lastPollDuration = lastTimePolled.HasValue ? Time.Current - lastTimePolled.Value : 0;

            scheduledPoll = Scheduler.AddDelayed(doPoll, Math.Max(0, TimeBetweenPolls.Value - lastPollDuration));
        }
    }
}
