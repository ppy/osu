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
    public abstract class TaikoHitObject : HitObject
    {
        /// <summary>
        /// To be honest, I don't know why this is needed. Old osu! scaled the
        /// slider multiplier by this factor, seemingly randomly, but now we unfortunately
        /// have to replicate that here anywhere slider length/slider multipliers are used :(
        /// </summary>
        public const double SLIDER_FUDGE_FACTOR = 1.4;
        
        public double HitWindowGreat;
        public double HitWindowGood;
        public double HitWindowMiss;

        public double PreEmpt;
        public float Scale = 1;
        public bool Kiai;

        public abstract TaikoHitType Type { get; }

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            Scale = 1f - 0.7f * -3f / 5 / 2;
            PreEmpt = 600 / (beatmap.SliderVelocityAt(StartTime) * SLIDER_FUDGE_FACTOR) * 1000;

            ControlPoint overridePoint;
            Kiai = beatmap.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;

            HitWindowGreat = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 50, 35, 20, Mods.None);
            HitWindowGood = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 120, 80, 50, Mods.None);
            HitWindowMiss = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 135, 95, 70, Mods.None);
        }
    }

    [Flags]
    public enum TaikoHitType
    {
        None = 0,
        Don = (1 << 0),
        Katsu = (1 << 1),
        DrumRoll = (1 << 2),
        DrumRollTick = (1 << 3),
        Bash = (1 << 4),
        Finisher = (1 << 5),

        HitCircle = Don | Katsu
    }
}
