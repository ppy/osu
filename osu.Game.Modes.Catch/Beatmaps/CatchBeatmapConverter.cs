// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Catch.Objects;
using System.Collections.Generic;
using System;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Beatmaps;

namespace osu.Game.Modes.Catch.Beatmaps
{
    internal class CatchBeatmapConverter : IBeatmapConverter<CatchBaseHit>
    {
        public IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        public Beatmap<CatchBaseHit> Convert(Beatmap original)
        {
            return new Beatmap<CatchBaseHit>(original)
            {
                HitObjects = new List<CatchBaseHit>() // Todo: Convert HitObjects
            };
        }
    }
}
