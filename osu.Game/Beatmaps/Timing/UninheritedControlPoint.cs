//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Timing
{
    class UninheritedControlPoint : ControlPoint
    {
        public double BeatLength { get; set; }
        public double BPM => 60000 / BeatLength;
    }
}
