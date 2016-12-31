//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Objects
{
    public abstract class HitObjectParser
    {
        public abstract HitObject Parse(string text);

        protected HitSampleInfo ParseHitSample(SampleInfo section, string sample, string addition)
        {
            HitSampleInfo hitSample = new HitSampleInfo
            {
                Type = (SampleType)Convert.ToInt32(sample, NumberFormatInfo.InvariantInfo) == SampleType.None ?
                    SampleType.Normal :
                    (SampleType)Convert.ToInt32(sample, NumberFormatInfo.InvariantInfo),
                Bank = section.Bank,
                Volume = section.Volume,
                File = null
            };

            string[] split = addition.Split(':');
            switch (split.Length)
            {
                case 5:
                    hitSample.File = string.IsNullOrWhiteSpace(split[4]) ? null : split[4];
                    goto case 4;
                case 4:
                    hitSample.Volume = Convert.ToInt32(split[3], NumberFormatInfo.InvariantInfo) == 0 ?
                        section.Volume :
                        Convert.ToInt32(split[3], NumberFormatInfo.InvariantInfo);
                    goto case 3;
                case 3:
                    hitSample.Bank = (SampleBank)Convert.ToInt32(split[2], NumberFormatInfo.InvariantInfo) == SampleBank.None ?
                        section.Bank :
                        (SampleBank)Convert.ToInt32(split[2], NumberFormatInfo.InvariantInfo);
                    goto case 2;
                case 2:
                    hitSample.Set = (SampleSet)Convert.ToInt32(split[0], NumberFormatInfo.InvariantInfo) == SampleSet.None ?
                        section.Set :
                        (SampleSet)Convert.ToInt32(split[0], NumberFormatInfo.InvariantInfo);
                    hitSample.AdditionSet = (SampleSet)Convert.ToInt32(split[1], NumberFormatInfo.InvariantInfo) == SampleSet.None ?
                        hitSample.Set :
                        (SampleSet)Convert.ToInt32(split[1], NumberFormatInfo.InvariantInfo);
                    break;
                default:
                    throw new ArgumentException("Additions must have between 2 and 5 values");
            }

            return hitSample;
        }
    }
}
