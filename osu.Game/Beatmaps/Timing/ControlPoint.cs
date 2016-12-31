//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps.Samples;

namespace osu.Game.Beatmaps.Timing
{
    public class ControlPoint
    {
        public double Time;
        public double Meter;
        public SampleInfo Sample;
        public bool Kiai;
    }
}
