// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        public double Time;
        public double BeatLength;

        /// <summary>
        /// This should only be used for Taiko conversion. Use <see cref="VelocityAdjustment"/> instead.
        /// </summary>
        public float FloatVelocityAdjustment;
        public double VelocityAdjustment;

        public bool TimingChange;

        public EffectFlags EffectFlags;
        
        public bool KiaiMode
        {
            get { return (EffectFlags & EffectFlags.Kiai) > 0; }
            set
            {
                if (value)
                    EffectFlags |= EffectFlags.Kiai;
                else
                    EffectFlags &= ~EffectFlags.Kiai;
            }
        }

        public bool OmitFirstBarLine
        {
            get { return (EffectFlags & EffectFlags.OmitFirstBarLine) > 0; }
            set
            {
                if (value)
                    EffectFlags |= EffectFlags.OmitFirstBarLine;
                else
                    EffectFlags &= ~EffectFlags.OmitFirstBarLine;
            }
        }
    }

    public enum EffectFlags
    {
        None = 0,
        Kiai = 1,
        OmitFirstBarLine = 8
    }

    internal enum TimeSignatures
    {
        SimpleQuadruple = 4,
        SimpleTriple = 3
    }
}
