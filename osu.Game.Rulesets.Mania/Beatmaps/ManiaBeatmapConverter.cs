// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmapConverter : BeatmapConverter<ManiaHitObject>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        protected override Beatmap<ManiaHitObject> ConvertBeatmap(Beatmap original, bool isForCurrentRuleset)
        {

            // Todo: This should be cased when we get better conversion methods
            var converter = new LegacyBeatmapConverter(original);

            return new Beatmap<ManiaHitObject>
            {
                BeatmapInfo = original.BeatmapInfo,
                TimingInfo = original.TimingInfo,
                // We need to sort here, because the converter generates patterns
                HitObjects = original.HitObjects.SelectMany(converter.Convert).OrderBy(h => h.StartTime).ToList()
            };
        }

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            var maniaOriginal = original as ManiaHitObject;
            if (maniaOriginal != null)
            {
                yield return maniaOriginal;
                yield break;
            }



            // Handled by the LegacyConvereter
            yield return null;
        }
    }
}
