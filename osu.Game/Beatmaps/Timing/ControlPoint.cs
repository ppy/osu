//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Beatmaps.Timing
{
    public class ControlPoint
    {
        public double Time;
        public double BeatLength;
        public double VelocityAdjustment;
        public bool TimingChange;
    }

    internal enum TimeSignatures
    {
        SimpleQuadruple = 4,
        SimpleTriple = 3
    }
}
