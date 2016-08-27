//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Timing
{
    /// <summary>
    /// A basic clock for keeping time.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// The current time of this clock, in milliseconds.
        /// </summary>
        double CurrentTime { get; }

        /// <summary>
        /// The rate this clock is running at, relative to real-time.
        /// </summary>
        double Rate { get; }

        /// <summary>
        /// Whether this clock is currently running or not.
        /// </summary>
        bool IsRunning { get; }
    }
}
