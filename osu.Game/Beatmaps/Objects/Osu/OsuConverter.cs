//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;

namespace osu.Game.Beatmaps.Objects.Osu
{
    class OsuConverter : HitObjectConverter<OsuBaseHit>
    {
        public override List<OsuBaseHit> Convert(List<HitObject> input)
        {
            List<OsuBaseHit> output = new List<OsuBaseHit>();

            foreach (HitObject h in input)
                output.Add(h as OsuBaseHit);

            return output;
        }
    }
}
