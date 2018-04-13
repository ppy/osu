// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        protected override IEnumerable<CatchHitObject> ConvertHitObject(HitObject obj, Beatmap beatmap)
        {
            var curveData = obj as IHasCurve;
            var positionData = obj as IHasXPosition;
            var comboData = obj as IHasCombo;
            var endTime = obj as IHasEndTime;

            if (positionData == null)
                yield break;

            if (curveData != null)
            {
                yield return new JuiceStream
                {
                    StartTime = obj.StartTime,
                    Samples = obj.Samples,
                    ControlPoints = curveData.ControlPoints,
                    CurveType = curveData.CurveType,
                    Distance = curveData.Distance,
                    RepeatSamples = curveData.RepeatSamples,
                    RepeatCount = curveData.RepeatCount,
                    X = positionData.X / CatchPlayfield.BASE_WIDTH,
                    NewCombo = comboData?.NewCombo ?? false
                };

                yield break;
            }

            if (endTime != null)
            {
                yield return new BananaShower
                {
                    StartTime = obj.StartTime,
                    Samples = obj.Samples,
                    Duration = endTime.Duration,
                    NewCombo = comboData?.NewCombo ?? false
                };

                yield break;
            }

            yield return new Fruit
            {
                StartTime = obj.StartTime,
                Samples = obj.Samples,
                NewCombo = comboData?.NewCombo ?? false,
                X = positionData.X / CatchPlayfield.BASE_WIDTH
            };
        }
    }
}
