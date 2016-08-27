//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Timing
{
    /// <summary>
    /// A clock which will only update its current time when a frame proces is triggered.
    /// Useful for keeping a consistent time state across an individual update.
    /// </summary>
    public interface IFrameBasedClock : IClock
    {
        double ElapsedFrameTime { get; }

        /// <summary>
        /// Processes one frame. Generally should be run once per update loop.
        /// </summary>
        void ProcessFrame();
    }
}
