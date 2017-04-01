﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Timing
{
    public class ControlPoint
    {
        public static ControlPoint Default = new ControlPoint
        {
            BeatLength = 500,
            TimingChange = true,
        };

        public TimeSignatures TimeSignature;
        public double Time;
        public double BeatLength;
        public double VelocityAdjustment;
        public bool TimingChange;
        public bool KiaiMode;
        public bool OmitFirstBarLine;
    }
}
