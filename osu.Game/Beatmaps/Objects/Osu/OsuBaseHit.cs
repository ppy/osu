//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Game.Beatmaps.Samples;

namespace osu.Game.Beatmaps.Objects.Osu
{
    public abstract class OsuBaseHit : HitObject
    {
        public Vector2 Position { get; set; }

        [Flags]
        private enum HitObjectType
        {
            Circle = 1,
            Slider = 2,
            NewCombo = 4,
            CircleNewCombo = 5,
            SliderNewCombo = 6,
            Spinner = 8,
            ColourHax = 122,
            Hold = 128,
            ManiaLong = 128,
        }

        public static OsuBaseHit Parse(string val)
        {
            string[] split = val.Split(',');
            var type = (HitObjectType)int.Parse(split[3]);
            bool combo = type.HasFlag(HitObjectType.NewCombo);
            type &= (HitObjectType)0xF;
            type &= ~HitObjectType.NewCombo;
            OsuBaseHit result;
            switch (type)
            {
                case HitObjectType.Circle:
                    result = new Circle();
                    break;
                case HitObjectType.Slider:
                    result = new Slider();
                    break;
                case HitObjectType.Spinner:
                    result = new Spinner();
                    break;
                default:
                    throw new InvalidOperationException($@"Unknown hit object type {type}");
            }
            result.Position = new Vector2(int.Parse(split[0]), int.Parse(split[1]));
            result.StartTime = double.Parse(split[2]);
            result.Sample = new HitSampleInfo { Type = (SampleType)int.Parse(split[4]) };
            result.NewCombo = combo;
            // TODO: "addition" field
            return result;
        }
    }
}
