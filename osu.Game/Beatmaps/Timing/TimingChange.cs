// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Timing
{
    class TimingChange : ControlPoint
    {
        public double BeatLength;

        public TimingChange(double beatLength)
        {
            BeatLength = beatLength;
        }

        public double BPM => 60000 / BeatLength;
    }
}
