using osu.Game.Modes.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Modes.Taiko.Objects
{
    public class TaikoHitObject : HitObject
    {
        public double PreEmpt;
        public float Scale = 1;
        public bool Kiai;

        public TaikoHitType Type => ((Sample?.Type ?? SampleType.None) & (~SampleType.Finish & ~SampleType.Normal)) == 0 ? TaikoHitType.Don : TaikoHitType.Katsu;
        public bool IsFinisher => ((Sample?.Type ?? SampleType.None) & SampleType.Finish) > 0;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            Scale = 1f - 0.7f * -3f / 5 / 2;
            PreEmpt = 600 / beatmap.SliderVelocityAt(StartTime) * 1000;

            ControlPoint overridePoint;
            Kiai = beatmap.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;
        }
    }

    public enum TaikoHitType
    {
        None,
        Don,
        Katsu,
    }
}
