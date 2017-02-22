// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko.Objects
{
    class TaikoConverter : HitObjectConverter<TaikoHitObject>
    {
        public override List<TaikoHitObject> Convert(Beatmap beatmap)
        {
            List<TaikoHitObject> output = new List<TaikoHitObject>();

            foreach (HitObject i in beatmap.HitObjects)
            {
                TaikoHitObject h = i as TaikoHitObject;

                if (h == null)
                {
                    OsuHitObject o = i as OsuHitObject;

                    if (o == null)
                        throw new HitObjectConvertException(@"Taiko", i);

                    if (o is Osu.Objects.HitCircle)
                    {
                        h = new HitCircle()
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            NewCombo = o.NewCombo,
                        };
                    }

                    if (o is Osu.Objects.Slider)
                    {
                        h = new DrumRoll()
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            NewCombo = o.NewCombo,
                            Length = (o as Osu.Objects.Slider).Length,
                            RepeatCount = (o as Osu.Objects.Slider).RepeatCount
                        };
                    }

                    if (o is Osu.Objects.Spinner)
                    {
                        h = new Spinner()
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            Length = (o as Osu.Objects.Spinner).Length
                        };
                    }
                }

                output.Add(h);
            }

            return output;
        }
    }
}
