// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Mania.Objects;
using System.Collections.Generic;
using System;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Beatmaps;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Mania.Beatmaps
{
    internal class ManiaBeatmapConverter : BeatmapConverter<ManiaBaseHit>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        protected override IEnumerable<ManiaBaseHit> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            yield return null;
        }
    }
}
