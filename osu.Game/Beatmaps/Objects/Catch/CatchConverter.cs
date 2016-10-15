//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps.Objects.Catch;
using osu.Game.Beatmaps.Objects.Osu;

namespace osu.Game.Beatmaps.Objects.Catch
{
    class CatchConverter : HitObjectConverter<CatchBaseHit>
    {
        public override List<CatchBaseHit> Convert(List<HitObject> input)
        {
            List<CatchBaseHit> output = new List<CatchBaseHit>();

            foreach (HitObject i in input)
            {
                CatchBaseHit h = i as CatchBaseHit;

                if (h == null)
                {
                    OsuBaseHit o = i as OsuBaseHit;

                    if (o == null) throw new HitObjectConvertException(@"Catch", i);

                    h = new Fruit
                    {
                        StartTime = o.StartTime,
                        Position = o.Position.X,
                    };
                }

                output.Add(h);
            }

            return output;
        }
    }
}
