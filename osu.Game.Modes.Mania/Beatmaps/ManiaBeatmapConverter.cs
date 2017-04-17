// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Mania.Objects;
using System.Collections.Generic;
using System;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Mania.Beatmaps
{
    internal class ManiaBeatmapConverter : IBeatmapConverter<ManiaBaseHit>
    {
        public IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasColumn) };

        public Beatmap<ManiaBaseHit> Convert(Beatmap original)
        {
            return new Beatmap<ManiaBaseHit>(original)
            {
                HitObjects = new List<ManiaBaseHit>() // Todo: Implement
            };
        }
    }
}
