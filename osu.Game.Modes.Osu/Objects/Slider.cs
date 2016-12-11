//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects
{
    public class Slider : OsuHitObject
    {
        public override double EndTime => StartTime + RepeatCount * Curve.Length / Velocity;

        public override Vector2 EndPosition => RepeatCount % 2 == 0 ? Position : Curve.PositionAt(1);

        public double Velocity;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            Velocity = 100 / beatmap.BeatLengthAt(StartTime, true) * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;
        }

        public int RepeatCount;

        public SliderCurve Curve;
    }

    public enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear,
        PerfectCurve
    }
}
