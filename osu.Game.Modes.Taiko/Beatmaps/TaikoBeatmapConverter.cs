// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Taiko.Objects;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko.Beatmaps
{
    internal class TaikoBeatmapConverter : IBeatmapConverter<TaikoHitObject>
    {
        public Beatmap<TaikoHitObject> Convert(Beatmap original)
        {
            return new Beatmap<TaikoHitObject>(original)
            {
                HitObjects = new List<TaikoHitObject>() // Todo: Implement
            };
        }
    }
}
