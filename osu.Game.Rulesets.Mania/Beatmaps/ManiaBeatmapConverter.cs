// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class ManiaBeatmapConverter : BeatmapConverter<ManiaHitObject>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            int availableColumns = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.CircleSize);

            var positionData = original as IHasXPosition;

            float localWDivisor = 512.0f / availableColumns;
            int column = MathHelper.Clamp((int)Math.Floor((positionData?.X ?? 1) / localWDivisor), 0, availableColumns - 1);

            yield return new Note
            {
                StartTime = original.StartTime,
                Column = column,
            };
        }
    }
}
