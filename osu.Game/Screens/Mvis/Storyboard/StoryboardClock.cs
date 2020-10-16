// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Timing;

namespace osu.Game.Screens.Mvis.Storyboard
{
    /// <summary>
    /// 这是一个更改过的<see cref="DecoupleableInterpolatingFramedClock"/>副本, 对应Framework版本`commit 596418a69d40c52bd83e0bc533aac2e51e1cf498`
    ///
    /// 改变了什么:
    /// `ChangeSource()`
    /// `Stop()`
    ///
    /// Adds the ability to keep the clock running even when the underlying source has stopped or cannot handle the current time range.
    /// This is handled by performing seeks on the underlying source and checking whether they were successful or not.
    /// On failure to seek, we take over with an internal clock until control can be returned to the actual source.
    ///
    /// This clock type removes the requirement of having a source set.
    ///
    /// If a <see cref="InterpolatingFramedClock.Source"/> is set, it is presumed that we have exclusive control over operations on it.
    /// This is used to our advantage to allow correct <see cref="IsRunning"/> state tracking in the event of cross-thread communication delays (with an audio thread, for instance).
    /// </summary>
    public class StoryboardClock : InterpolatingFramedClock, IAdjustableClock
    {
        /// <summary>
        /// Specify whether we are coupled 1:1 to SourceClock. If not, we can independently continue operation.
        /// </summary>
        public bool IsCoupled = true;

        /// <summary>
        /// In some cases we should always use the interpolated source.
        /// </summary>
        private bool useInterpolatedSourceTime => IsRunning && FramedSourceClock?.IsRunning == true;

        private readonly FramedClock decoupledClock;
        private readonly StopwatchClock decoupledStopwatch;

        /// <summary>
        /// We need to be able to pass on adjustments to the source if it supports them.
        /// </summary>
        private IAdjustableClock adjustableSource => Source as IAdjustableClock;

        public override double CurrentTime => currentTime;

        private double currentTime;

        public double ProposedCurrentTime => useInterpolatedSourceTime ? base.CurrentTime : decoupledClock.CurrentTime;

        public double ProposedElapsedTime => useInterpolatedSourceTime ? base.ElapsedFrameTime : decoupledClock.ElapsedFrameTime;

        public override bool IsRunning => decoupledClock.IsRunning; // we always want to use our local IsRunning state, as it is more correct.

        private double elapsedFrameTime;

        public override double ElapsedFrameTime => elapsedFrameTime;

        public override double Rate
        {
            get => Source?.Rate ?? 1;
            set => adjustableSource.Rate = value;
        }

        public void ResetSpeedAdjustments() => Rate = 1;

        public StoryboardClock()
        {
            decoupledClock = new FramedClock(decoupledStopwatch = new StopwatchClock());
        }

        public override void ProcessFrame()
        {
            base.ProcessFrame();

            bool sourceRunning = Source?.IsRunning ?? false;

            decoupledStopwatch.Rate = adjustableSource?.Rate ?? 1;

            // if interpolating based on the source, keep the decoupled clock in sync with the interpolated time.
            if (IsCoupled && sourceRunning)
                decoupledStopwatch.Seek(base.CurrentTime);

            // process the decoupled clock to update the current proposed time.
            decoupledClock.ProcessFrame();

            // if the source clock is started as a result of becoming capable of handling the decoupled time, the proposed time may change to reflect the interpolated source time.
            // however the interpolated source time that was calculated inside base.ProcessFrame() (above) did not consider the current (post-seek) time of the source.
            // in all other cases the proposed time will match before and after clocks are started/stopped.
            double proposedTime = ProposedCurrentTime;
            double elapsedTime = ProposedElapsedTime;

            if (IsRunning)
            {
                if (IsCoupled)
                {
                    // when coupled, we want to stop when our source clock stops.
                    if (!sourceRunning)
                        Stop();
                }
                else
                {
                    // when decoupled and running, we should try to start the source clock it if it's capable of handling the current time.
                    if (!sourceRunning)
                        Start();
                }
            }
            else if (IsCoupled && sourceRunning)
            {
                // when coupled and not running, we want to start when the source clock starts.
                Start();
            }

            elapsedFrameTime = elapsedTime;

            // the source may be started during playback but remain behind the current time in the playback direction for a number of frames.
            // in such cases, the current time should remain paused until the source time catches up.
            currentTime = elapsedFrameTime < 0 ? Math.Min(currentTime, proposedTime) : Math.Max(currentTime, proposedTime);
        }

        public override void ChangeSource(IClock source)
        {
            if (source == null) return;

            Seek( (source as IAdjustableClock).CurrentTime );

            base.ChangeSource(source);
        }

        public void Reset()
        {
            IsCoupled = true;

            adjustableSource?.Reset();
            decoupledStopwatch.Reset();
        }

        public void Start()
        {
            if (adjustableSource?.IsRunning == false)
            {
                if (adjustableSource.Seek(ProposedCurrentTime))
                    //only start the source clock if our time values match.
                    //this handles the case where we seeked to an unsupported value and the source clock is out of sync.
                    adjustableSource.Start();
            }

            decoupledStopwatch.Start();
        }

        public void Stop()
        {
            decoupledStopwatch.Stop();
        }

        public bool Seek(double position)
        {
            try
            {
                return decoupledStopwatch.Seek(position);
            }
            finally
            {
                ProcessFrame();
            }
        }
    }
}
