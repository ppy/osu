//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace osu.Framework.Timing
{
    public class StopwatchClock : Stopwatch, IAdjustableClock
    {
        private double seekOffset;

        /// <summary>
        /// Keep track of how much stopwatch time we have used at previous rates.
        /// </summary>
        private double rateChangeUsed;

        /// <summary>
        /// Keep track of the resultant time that was accumulated at previous rates.
        /// </summary>
        private double rateChangeAccumulated;

        public StopwatchClock(bool start = false)
        {
            if (start)
                Start();
        }

        public double CurrentTime => (stopwatchMilliseconds - rateChangeUsed) * rate + rateChangeAccumulated + seekOffset;

        private double stopwatchMilliseconds => (double)ElapsedTicks / Frequency * 1000;

        private double rate = 1;
        public double Rate
        {
            get
            {
                return rate;
            }

            set
            {
                if (rate == value) return;

                rateChangeAccumulated += (stopwatchMilliseconds - rateChangeUsed) * rate;
                rateChangeUsed = stopwatchMilliseconds;
                
                rate = value;
            }
        }

        public bool Seek(double position)
        {
            seekOffset = 0;
            seekOffset = position - CurrentTime;
            return true;
        }
    }
}
