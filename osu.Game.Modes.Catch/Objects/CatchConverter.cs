// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Catch.Objects
{
    class CatchConverter : HitObjectConverter<CatchBaseHit>
    {
        public override List<CatchBaseHit> Convert(Beatmap beatmap)
        {
            List<CatchBaseHit> output = new List<CatchBaseHit>();

            foreach (HitObject i in beatmap.HitObjects)
            {
                CatchBaseHit h = i as CatchBaseHit;

                if (h == null)
                {
                    OsuHitObject o = i as OsuHitObject;

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
