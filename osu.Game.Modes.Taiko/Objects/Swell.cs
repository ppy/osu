// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Taiko.Objects
{
    public class Swell : TaikoHitObject, IHasEndTime
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        /// <summary>
        /// The multiplier for cases in which the number of required hits by a Swell is not
        /// dependent on solely the overall difficulty and the duration of the swell.
        /// </summary>
        public double HitMultiplier { get; set; } = 1;

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits { get; protected set; } = 10;

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            double spinnerRotationRatio = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 3, 5, 7.5);
            RequiredHits = (int)Math.Max(1, Duration / 1000f * spinnerRotationRatio * HitMultiplier);
        }
    }
}