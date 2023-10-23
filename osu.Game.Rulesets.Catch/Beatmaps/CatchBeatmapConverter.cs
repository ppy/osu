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
            var xPositionData = obj as IHasXPosition;
            var yPositionData = obj as IHasYPosition;
            var comboData = obj as IHasCombo;
            var sliderVelocityData = obj as IHasSliderVelocity;

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
                        X = xPositionData?.X ?? 0,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                        LegacyConvertedY = yPositionData?.Y ?? CatchHitObject.DEFAULT_LEGACY_CONVERT_Y,
                        SliderVelocityMultiplier = sliderVelocityData?.SliderVelocityMultiplier ?? 1
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
                        X = xPositionData?.X ?? 0,
                        LegacyConvertedY = yPositionData?.Y ?? CatchHitObject.DEFAULT_LEGACY_CONVERT_Y
                    }.Yield();
            }
        }

        protected override Beatmap<CatchHitObject> CreateBeatmap() => new CatchBeatmap();
    }
}
