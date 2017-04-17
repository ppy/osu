// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class ManiaBeatmapConverter : IBeatmapConverter<ManiaBaseHit>
    {
        public Beatmap<ManiaBaseHit> Convert(Beatmap original)
        {
            return new Beatmap<ManiaBaseHit>(original)
            {
                HitObjects = new List<ManiaBaseHit>() // Todo: Implement
            };
        }
    }
}
