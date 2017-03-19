// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
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
        public double HitWindowGreat = 35;

        /// <summary>
        /// The hit window that results in a "GOOD" hit.
        /// </summary>
        public double HitWindowGood = 80;

        /// <summary>
        /// The hit window that results in a "MISS".
        /// </summary>
        public double HitWindowMiss = 95;

        /// <summary>
        /// The time to scroll in the HitObject.
        /// </summary>
        public double PreEmpt;

        /// <summary>
        /// Whether this HitObject is in Kiai time.
        /// </summary>
        public bool Kiai;

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            PreEmpt = 600 / (timing.SliderVelocityAt(StartTime) * difficulty.SliderMultiplier) * 1000;

            ControlPoint overridePoint;
            Kiai = timing.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;

            HitWindowGreat = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 50, 35, 20);
            HitWindowGood = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 120, 80, 50);
            HitWindowMiss = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 135, 95, 70);
        }
    }
}