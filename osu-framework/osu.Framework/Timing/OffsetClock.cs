//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Timing
{
    public class OffsetClock : IClock
    {
        protected IClock Source;

        public double Offset;

        public double CurrentTime => Source.CurrentTime + Offset;

        public double Rate => Source.Rate;

        public bool IsRunning => Source.IsRunning;

        public OffsetClock(IClock source)
        {
            this.Source = source;
        }
    }
}
