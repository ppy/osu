//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;

namespace osu.Game.Modes.Taiko.Objects
{
    class TaikoConverter : HitObjectConverter<TaikoBaseHit>
    {
        public override List<TaikoBaseHit> Convert(List<HitObject> input)
        {
            List<TaikoBaseHit> output = new List<TaikoBaseHit>();

            foreach (HitObject i in input)
            {
                TaikoBaseHit h = i as TaikoBaseHit;

                if (h == null)
                {
                    OsuHitObject o = i as OsuHitObject;

                    if (o == null) throw new HitObjectConvertException(@"Taiko", i);

                    h = new TaikoBaseHit
                    {
                        StartTime = o.StartTime,
                    };
                }

                output.Add(h);
            }

            return output;
        }
    }
}
