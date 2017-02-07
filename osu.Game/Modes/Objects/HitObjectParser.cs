//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Globalization;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;

namespace osu.Game.Modes.Objects
{
    public abstract class HitObjectParser
    {
        public abstract HitObject Parse(Beatmap beatmap, string text);

        public class UnknownHitObjectException : Exception
        {
            public int Type { get; }
            public UnknownHitObjectException(int type)
                : base($@"Unknown HitObject type {type}")
            {
                Type = type;
            }
        }

        protected HitSampleInfo ParseHitSample(SampleInfo section, string sample = null, string addition = "0:0")
        {
            HitSampleInfo hitSample = new HitSampleInfo
            {
                Type = (SampleType)Convert.ToInt32(sample, NumberFormatInfo.InvariantInfo) | SampleType.Normal,
                Bank = section.Bank,
                Volume = section.Volume,
                File = null
            };

            string[] split = addition.Split(':');
            if (split.Length < 2)
                split = "0:0".Split(':');
            if (split.Length > 5)
                Array.Resize(ref split, 5);
            for (int i = 0; i < split.Length; i++)
            {
                int value;
                int.TryParse(split[i], NumberStyles.None, NumberFormatInfo.InvariantInfo, out value);
                switch (i)
                {
                    case 0:
                        SampleSet hitSampleSet = (SampleSet)value;
                        hitSample.Set = hitSampleSet == SampleSet.None ? section.Set : hitSampleSet;
                        break;
                    case 1:
                        SampleSet hitAdditionSet = (SampleSet)value;
                        hitSample.AdditionSet = hitAdditionSet == SampleSet.None ? hitSample.Set : hitAdditionSet;
                        break;
                    case 2:
                        SampleBank hitSampleBank = (SampleBank)value;
                        if (hitSampleBank != SampleBank.None)
                            hitSample.Bank = hitSampleBank;
                        break;
                    case 3:
                        if (value != 0)
                            hitSample.Volume = value;
                        break;
                    case 4:
                        if (!string.IsNullOrWhiteSpace(split[i]))
                            hitSample.File = split[i];
                        break;
                }
            }

            return hitSample;
        }
    }
}
