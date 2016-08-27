//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.Timing
{
    /// <summary>
    /// Adds the ability to keep the clock running even when the underlying source has stopped or cannot handle the current time range.
    /// This is handled by performing seeks on the underlying source and checking whether they were successful or not.
    /// On failure to seek, we take over with an internal clock until control can be returned to the actual source.
    /// 
    /// This clock type removes the requirement of having a source set.
    /// </summary>
    public class DecoupleableInterpolatingFramedClock : InterpolatingFramedClock, IAdjustableClock
    {
        /// <summary>
        /// Specify whether we are coupled 1:1 to SourceClock. If not, we can independently continue operation.
        /// </summary>
        public bool IsCoupled = true;

        private bool useDecoupledClock => SourceClock == null || (!IsCoupled && !SourceClock.IsRunning);

        private FramedClock decoupledClock;
        private StopwatchClock decoupledStopwatch;

        /// <summary>
        /// We need to be able to pass on adjustments to the source if it supports them.
        /// </summary>
        private IAdjustableClock adjustableSource => SourceClock?.Source as IAdjustableClock;

        public override double CurrentTime => useDecoupledClock ? decoupledClock.CurrentTime : base.CurrentTime;

        public override bool IsRunning => useDecoupledClock ? decoupledClock.IsRunning : base.IsRunning;

        public override double ElapsedFrameTime => useDecoupledClock ? decoupledClock.ElapsedFrameTime : base.ElapsedFrameTime;

        public DecoupleableInterpolatingFramedClock()
        {
            decoupledClock = new FramedClock(decoupledStopwatch = new StopwatchClock());
        }

        public override void ProcessFrame()
        {
            base.ProcessFrame();

            decoupledStopwatch.Rate = adjustableSource?.Rate ?? 1;
            decoupledClock.ProcessFrame();

            bool sourceRunning = SourceClock?.IsRunning ?? false;

            if (IsCoupled || sourceRunning)
            {
                if (sourceRunning)
                    decoupledStopwatch.Start();
                else
                    decoupledStopwatch.Stop();

                decoupledStopwatch.Seek(CurrentTime);
            }
            else
            {
                if (decoupledClock.IsRunning)
                {
                    //if we're running but our source isn't, we should try a seek to see if it's capable to switch to it for the current value.
                    if (adjustableSource?.Seek(CurrentTime) == true)
                        Start();
                }
            }
        }

        public void Reset()
        {
            IsCoupled = true;

            adjustableSource?.Reset();
            decoupledStopwatch.Reset();
        }

        public void Start()
        {
            if (IsCoupled || adjustableSource?.Seek(CurrentTime) == true)
                //only start the source clock if our time values match.
                //this handles the case where we seeked to an unsupported value and the source clock is out of sync.
                adjustableSource?.Start();
            decoupledStopwatch.Start();
        }

        public void Stop()
        {
            adjustableSource?.Stop();
            decoupledStopwatch.Stop();
        }

        public bool Seek(double position)
        {
            bool success = adjustableSource?.Seek(position) == true;

            if (IsCoupled)
            {
                decoupledStopwatch.Seek(adjustableSource?.CurrentTime ?? position);
                return success;
            }
            else
            {
                if (!success)
                    //if we failed to seek then stop the source and use decoupled mode.
                    adjustableSource?.Stop();

                return decoupledStopwatch.Seek(position);
            }
        }
    }
}
