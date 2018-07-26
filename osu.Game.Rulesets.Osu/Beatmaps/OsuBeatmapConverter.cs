// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    internal class OsuBeatmapConverter : BeatmapConverter<OsuHitObject>
    {
        public OsuBeatmapConverter(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasPosition) };

        protected override IEnumerable<OsuHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            var curveData = original as IHasCurve;
            var endTimeData = original as IHasEndTime;
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;
            var legacyOffset = original as IHasLegacyLastTickOffset;

            if (curveData != null)
            {
                yield return new Slider
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    ControlPoints = curveData.ControlPoints,
                    CurveType = curveData.CurveType,
                    Distance = curveData.Distance,
                    RepeatSamples = curveData.RepeatSamples,
                    RepeatCount = curveData.RepeatCount,
                    Position = positionData?.Position ?? Vector2.Zero,
                    NewCombo = comboData?.NewCombo ?? false,
                    LegacyLastTickOffset = legacyOffset?.LegacyLastTickOffset
                };
            }
            else if (endTimeData != null)
            {
                yield return new Spinner
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    EndTime = endTimeData.EndTime,
                    Position = positionData?.Position ?? OsuPlayfield.BASE_SIZE / 2
                };
            }
            else
            {
                yield return new HitCircle
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    Position = positionData?.Position ?? Vector2.Zero,
                    NewCombo = comboData?.NewCombo ?? false
                };
            }
        }

        protected override Beatmap<OsuHitObject> CreateBeatmap() => new OsuBeatmap();
    }
}
