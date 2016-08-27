//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Timing
{
    /// <summary>
    /// A completely manual clock implementation. Everything is settable.
    /// </summary>
    public class ManualClock : IClock
    {
        public double CurrentTime { get; set; }
        public double Rate { get; set; }
        public bool IsRunning { get; set; }
    }
}
