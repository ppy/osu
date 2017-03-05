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
        public double HitWindowGreat;
        public double HitWindowGood;
        public double HitWindowMiss;

        public double PreEmpt;
        public float Scale = 1;
        public bool Kiai;

        public TaikoHitType Type => ((Sample?.Type ?? SampleType.None) & (~SampleType.Finish & ~SampleType.Normal)) == 0 ? TaikoHitType.Don : TaikoHitType.Katsu;
        public bool IsFinisher => ((Sample?.Type ?? SampleType.None) & SampleType.Finish) > 0;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            // Don't ask... Old osu! had a random multiplier here, that we now have to multiply everywhere
            float fudgeFactor = 1.4f;

            Scale = 1f - 0.7f * -3f / 5 / 2;
            PreEmpt = 600 / (beatmap.SliderVelocityAt(StartTime) * fudgeFactor) * 1000;

            ControlPoint overridePoint;
            Kiai = beatmap.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;

            HitWindowGreat = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 50, 35, 20, Mods.None);
            HitWindowGood = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 120, 80, 50, Mods.None);
            HitWindowMiss = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 135, 95, 70, Mods.None);
        }
    }

    public enum TaikoHitType
    {
        None,
        Don,
        Katsu,
    }
}
