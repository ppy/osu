// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
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

        private double timeBetweenPolls;

        /// <summary>
        /// The time in milliseconds to wait between polls.
        /// Setting to zero stops all polling.
        /// </summary>
        public double TimeBetweenPolls
        {
            get => timeBetweenPolls;
            set
            {
                timeBetweenPolls = value;
                scheduledPoll?.Cancel();
                pollIfNecessary();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="timeBetweenPolls">The initial time in milliseconds to wait between polls. Setting to zero stops all polling.</param>
        protected PollingComponent(double timeBetweenPolls = 0)
        {
            TimeBetweenPolls = timeBetweenPolls;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            pollIfNecessary();
        }

        private bool pollIfNecessary()
        {
            // we must be loaded so we have access to clock.
            if (!IsLoaded) return false;

            // there's already a poll process running.
            if (pollingActive) return false;

            // don't try polling if the time between polls hasn't been set.
            if (timeBetweenPolls == 0) return false;

            if (!lastTimePolled.HasValue)
            {
                doPoll();
                return true;
            }

            if (Time.Current - lastTimePolled.Value > timeBetweenPolls)
            {
                doPoll();
                return true;
            }

            // not ennough time has passed since the last poll. we do want to schedule a poll to happen, though.
            scheduleNextPoll();
            return false;
        }

        private void doPoll()
        {
            scheduledPoll = null;
            pollingActive = true;
            Poll().ContinueWith(_ => pollComplete());
        }

        /// <summary>
        /// Performs a poll. Implement but do not call this.
        /// </summary>
        protected virtual Task Poll()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Immediately performs a <see cref="Poll"/>.
        /// </summary>
        public void PollImmediately()
        {
            lastTimePolled = Time.Current - timeBetweenPolls;
            scheduleNextPoll();
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

        private void scheduleNextPoll()
        {
            scheduledPoll?.Cancel();

            double lastPollDuration = lastTimePolled.HasValue ? Time.Current - lastTimePolled.Value : 0;

            scheduledPoll = Scheduler.AddDelayed(doPoll, Math.Max(0, timeBetweenPolls - lastPollDuration));
        }
    }
}
