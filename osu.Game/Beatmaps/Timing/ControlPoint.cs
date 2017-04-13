// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Timing
{
    public class ControlPoint
    {
        public string SampleBank;
        public int SampleVolume;
        public TimeSignatures TimeSignature;
        public double Time;
        public double BeatLength = 500;
        public double SpeedMultiplier = 1;
        public bool TimingChange = true;
        public bool KiaiMode;
        public bool OmitFirstBarLine;

        public ControlPoint Clone() => (ControlPoint)MemberwiseClone();
    }
}
