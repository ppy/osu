// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapConverter : BeatmapConverter<CatchHitObject>
    {
        public CatchBeatmapConverter(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        protected override IEnumerable<CatchHitObject> ConvertHitObject(HitObject obj, IBeatmap beatmap)
        {
            var curveData = obj as IHasCurve;
            var positionData = obj as IHasXPosition;
            var comboData = obj as IHasCombo;
            var endTime = obj as IHasEndTime;
            var legacyOffset = obj as IHasLegacyLastTickOffset;

            if (curveData != null)
            {
                yield return new JuiceStream
                {
                    StartTime = obj.StartTime,
                    Samples = obj.Samples,
                    Path = curveData.Path,
                    NodeSamples = curveData.NodeSamples,
                    RepeatCount = curveData.RepeatCount,
                    X = (positionData?.X ?? 0) / CatchPlayfield.BASE_WIDTH,
                    NewCombo = comboData?.NewCombo ?? false,
                    ComboOffset = comboData?.ComboOffset ?? 0,
                    LegacyLastTickOffset = legacyOffset?.LegacyLastTickOffset ?? 0
                };
            }
            else if (endTime != null)
            {
                yield return new BananaShower
                {
                    StartTime = obj.StartTime,
                    Samples = obj.Samples,
                    Duration = endTime.Duration,
                    NewCombo = comboData?.NewCombo ?? false,
                    ComboOffset = comboData?.ComboOffset ?? 0,
                };
            }
            else
            {
                yield return new Fruit
                {
                    StartTime = obj.StartTime,
                    Samples = obj.Samples,
                    NewCombo = comboData?.NewCombo ?? false,
                    ComboOffset = comboData?.ComboOffset ?? 0,
                    X = (positionData?.X ?? 0) / CatchPlayfield.BASE_WIDTH
                };
            }
        }

        protected override Beatmap<CatchHitObject> CreateBeatmap() => new CatchBeatmap();
    }
}
