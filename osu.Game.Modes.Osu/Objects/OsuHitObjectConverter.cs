//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Osu.Objects
{
    public class OsuHitObjectConverter : HitObjectConverter<OsuHitObject>
    {
        public override List<OsuHitObject> Convert(List<HitObject> input)
        {
            List<OsuHitObject> output = new List<OsuHitObject>();

            foreach (HitObject h in input)
                output.Add(h as OsuHitObject);

            return output;
        }
    }
}
