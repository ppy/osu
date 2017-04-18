// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    internal class CatchBeatmapConverter : IBeatmapConverter<CatchBaseHit>
    {
        public Beatmap<CatchBaseHit> Convert(Beatmap original)
        {
            return new Beatmap<CatchBaseHit>(original)
            {
                HitObjects = new List<CatchBaseHit>() // Todo: Convert HitObjects
            };
        }
    }
}
