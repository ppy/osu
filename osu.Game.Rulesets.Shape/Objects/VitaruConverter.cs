using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Vitaru.Objects.Characters;

namespace osu.Game.Modes.Vitaru.Objects
{
    internal class VitaruConverter : HitObjectConverter<VitaruHitObject>
    {
        public override List<VitaruHitObject> Convert(Beatmap beatmap)
        {
            List<VitaruHitObject> output = new List<VitaruHitObject>();

            foreach (HitObject i in beatmap.HitObjects)
            {
                VitaruHitObject h = i as VitaruHitObject;

                if (h == null)
                {
                    OsuHitObject o = i as OsuHitObject;

                    if (o == null) throw new HitObjectConvertException(@"Vitaru", i);

                    h = new Enemy();
                }
                output.Add(h);
            }
            return output;
        }
    }
}