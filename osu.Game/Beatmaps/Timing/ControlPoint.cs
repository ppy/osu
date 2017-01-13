//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;

namespace osu.Game.Beatmaps.Timing
{
    public class ControlPoint
    {
        public double Time { get; set; }
        public double Meter { get; set; }
        public SampleInfo Sample { get; set; }
        public KiaiType Kiai { get; set; }
    }
}
