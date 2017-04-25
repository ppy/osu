// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps
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
