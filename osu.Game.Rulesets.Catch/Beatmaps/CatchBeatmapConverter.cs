// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapConverter : BeatmapConverter<CatchHitObject>
    {
        public CatchBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasXPosition);

        protected override IEnumerable<CatchHitObject> ConvertHitObject(HitObject obj, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var positionData = obj as IHasXPosition;
            var comboData = obj as IHasCombo;

            switch (obj)
            {
                case IHasPathWithRepeats curveData:
                    return new JuiceStream
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples,
                        Path = curveData.Path,
                        NodeSamples = curveData.NodeSamples,
                        RepeatCount = curveData.RepeatCount,
                        X = positionData?.X ?? 0,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                        LegacyLastTickOffset = (obj as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0
                    }.Yield();

                case IHasDuration endTime:
                    return new BananaShower
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples,
                        Duration = endTime.Duration,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                    }.Yield();

                default:
                    return new Fruit
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                        X = positionData?.X ?? 0
                    }.Yield();
            }
        }

        protected override Beatmap<CatchHitObject> CreateBeatmap() => new CatchBeatmap();
    }
}
