// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using System;
using System.Globalization;

namespace osu.Game.Modes.Taiko.Objects
{
    public class TaikoHitObjectParser : HitObjectParser
    {
        public override HitObject Parse(string text)
        {
            string[] split = text.Split(',');
            var type = (HitObjectType)int.Parse(split[3]);
            bool combo = type.HasFlag(HitObjectType.NewCombo);
            type &= (HitObjectType)0xF;
            type &= ~HitObjectType.NewCombo;

            TaikoHitObject result;
            switch (type)
            {
                case HitObjectType.Circle:
                    result = new HitCircle();
                    break;
                case HitObjectType.Slider:
                    int repeatCount = repeatCount = Convert.ToInt32(split[6], CultureInfo.InvariantCulture);

                    if (repeatCount > 9000)
                        throw new ArgumentOutOfRangeException("wacky man");

                    result = new DrumRoll
                    {
                        Length = Convert.ToDouble(split[7], CultureInfo.InvariantCulture) * repeatCount
                    };
                    break;
                case HitObjectType.Spinner:
                    result = new Bash
                    {
                        Length = Convert.ToDouble(split[5], CultureInfo.InvariantCulture) - Convert.ToDouble(split[2], CultureInfo.InvariantCulture),
                    };
                    break;
                default:
                    throw new InvalidOperationException($@"Unknown hit object type {type}");
            }

            result.NewCombo = combo;
            result.StartTime = Convert.ToDouble(split[2], CultureInfo.InvariantCulture);
            result.Sample = new HitSampleInfo
            {
                Type = (SampleType)int.Parse(split[4]),
                Set = SampleSet.Soft,
            };
            // TODO: "addition" field

            return result;
        }
    }
}
