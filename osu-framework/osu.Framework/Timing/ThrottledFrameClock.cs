//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace osu.Framework.Timing
{
    /// <summary>
    /// A FrameClock which will limit the number of frames processed by adding Thread.Sleep calls on each ProcessFrame.
    /// </summary>
    public class ThrottledFrameClock : FramedClock
    {
        /// <summary>
        /// The number of updated per second which is permitted.
        /// </summary>
        public int MaximumUpdateHz = 1000;

        /// <summary>
        /// If true, we will perform a Thread.Sleep even if the period is absolute zero.
        /// Allows other threads to process.
        /// </summary>
        public bool AlwaysSleep = true;

        private double minimumFrameTime => 1000d / MaximumUpdateHz;


        public ThrottledFrameClock()
            : base(new StopwatchClock(true))
        {

        }

        double averageFrameTime;

        public override void ProcessFrame()
        {
            double rawFrameTime = SourceTime - LastFrameTime;

            //average frame time over the last 5 frames.
            averageFrameTime = averageFrameTime == 0 ? rawFrameTime : (averageFrameTime * 4 + rawFrameTime) / 5;

            double sleepTime = Math.Max(0, minimumFrameTime - averageFrameTime);
            if (sleepTime > 0 || AlwaysSleep)
                Thread.Sleep(new TimeSpan((int)(sleepTime / 1000 * Stopwatch.Frequency)));

            base.ProcessFrame();
        }
    }
}
