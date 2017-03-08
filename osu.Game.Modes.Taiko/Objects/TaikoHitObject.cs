// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Taiko.Objects
{
    public class TaikoHitObject : HitObject
    {
        /// <summary>
        /// HitCircle radius.
        /// </summary>
        public const float CIRCLE_RADIUS = 64;
        
        /// <summary>
        /// The hit window that results in a "GREAT" hit.
        /// </summary>
        public double HitWindowGreat;

        /// <summary>
        /// The hit window that results in a "GOOD" hit.
        /// </summary>
        public double HitWindowGood;

        /// <summary>
        /// The hit window that results in a "MISS".
        /// </summary>
        public double HitWindowMiss;

        /// <summary>
        /// The time to scroll in the HitObject.
        /// </summary>
        public double PreEmpt;

        /// <summary>
        /// Whether this HitObject is in Kiai time.
        /// </summary>
        public bool Kiai;

        /// <summary>
        /// The type of HitObject.
        /// </summary>
        public virtual TaikoHitType Type
        {
            get
            {
                SampleType st = Sample?.Type ?? SampleType.None;

                return
                    // Centre/Rim
                    ((st & ~(SampleType.Finish | SampleType.Normal)) == 0 ? TaikoHitType.CentreHit : TaikoHitType.RimHit)
                    // Finisher
                    | ((st & SampleType.Finish) > 0 ? TaikoHitType.Finisher : TaikoHitType.None);
            }
        }

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            PreEmpt = 600 / beatmap.SliderVelocityAt(StartTime) * 1000;

            ControlPoint overridePoint;
            Kiai = beatmap.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;

            HitWindowGreat = Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 50, 35, 20, Mods.None);
            HitWindowGood = Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 120, 80, 50, Mods.None);
            HitWindowMiss = Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 135, 95, 70, Mods.None);
        }
    }

    [Flags]
    public enum TaikoHitType
    {
        None = 0,
        CentreHit = 1 << 0,
        RimHit = 1 << 1,
        DrumRoll = 1 << 2,
        DrumRollTick = 1 << 3,
        Bash = 1 << 4,
        Finisher = 1 << 5,

        Hit = CentreHit | RimHit
    }
}
