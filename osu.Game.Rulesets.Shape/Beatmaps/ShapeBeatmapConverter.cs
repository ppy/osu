using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Shape.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Audio;
using System.Linq;

namespace osu.Game.Rulesets.Shape.Beatmaps
{
    internal class ShapeBeatmapConverter : BeatmapConverter<ShapeHitObject>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasPosition) };

        protected override IEnumerable<ShapeHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            var curveData = original as IHasCurve;
            var endTimeData = original as IHasEndTime;
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;

            List<SampleInfo> samples = original.Samples;

            bool isSquare = samples.Any(s => s.Name == SampleInfo.HIT_WHISTLE);
            bool isTriangle = samples.Any(s => s.Name == SampleInfo.HIT_FINISH);
            bool isX = samples.Any(s => s.Name == SampleInfo.HIT_CLAP);

            if (isSquare)
            {
                yield return new BaseShape
                {
                    StartTime = original.StartTime,
                    StartPosition = positionData?.Position ?? Vector2.Zero,
                    Samples = original.Samples,
                    NewCombo = comboData?.NewCombo ?? false,
                    ShapeID = 2,
                };
            }
            else if (isTriangle)
            {
                yield return new BaseShape
                {
                    StartTime = original.StartTime,
                    StartPosition = positionData?.Position ?? Vector2.Zero,
                    Samples = original.Samples,
                    NewCombo = comboData?.NewCombo ?? false,
                    ShapeID = 3,
                };
            }
            else if (isX)
            {
                yield return new BaseShape
                {
                    StartTime = original.StartTime,
                    StartPosition = positionData?.Position ?? Vector2.Zero,
                    Samples = original.Samples,
                    NewCombo = comboData?.NewCombo ?? false,
                    ShapeID = 4,
                };
            }
            else
            {
                yield return new BaseShape
                {
                    StartTime = original.StartTime,
                    StartPosition = positionData?.Position ?? Vector2.Zero,
                    Samples = original.Samples,
                    NewCombo = comboData?.NewCombo ?? false,
                    ShapeID = 1,
                };
            }
        }
    }
}
