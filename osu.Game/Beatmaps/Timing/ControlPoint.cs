// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

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

        public EffectFlags EffectFlags;
        
        public bool KiaiMode => (EffectFlags & EffectFlags.Kiai) > 0;
    }

    [Flags]
    public enum EffectFlags
    {
        None = 0,
        Kiai = 1,
        OmitFirstBarLine = 8,
        PostProcessed = 16384,
    }

    public enum TimeSignatures
    {
        SimpleQuadruple = 4,
        SimpleTriple = 3
    }
}
