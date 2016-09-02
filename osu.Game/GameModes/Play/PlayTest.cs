//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.GameModes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;

namespace osu.Game.GameModes.Play
{
    class PlayTest : GameMode
    {
        public override void Load()
        {
            base.Load();

            Beatmap beatmap = new Beatmap();

            beatmap.HitObjects = new List<Beatmaps.Objects.BaseHit>()
            {
                new HitObject() {  },
            };

        }
    }
}
