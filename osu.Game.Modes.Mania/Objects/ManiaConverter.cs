// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Mania.Objects
{
    class ManiaConverter : HitObjectConverter<ManiaBaseHit>
    {
        private readonly int columns;

        public ManiaConverter(int columns)
        {
            this.columns = columns;
        }

        public override List<ManiaBaseHit> Convert(Beatmap beatmap)
        {
            List<ManiaBaseHit> output = new List<ManiaBaseHit>();

            foreach (HitObject i in beatmap.HitObjects)
            {
                ManiaBaseHit h = i as ManiaBaseHit;

                if (h == null)
                {
                    OsuHitObject o = i as OsuHitObject;

                    if (o == null) throw new HitObjectConvertException(@"Mania", i);

                    h = new Note
                    {
                        StartTime = o.StartTime,
                        Column = (int)Math.Round(o.Position.X / 512 * columns)
                    };
                }

                output.Add(h);
            }

            return output;
        }
    }
}
