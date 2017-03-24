// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Taiko.Objects
{
    public class Bash : TaikoHitObject, IHasEndTime
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        /// <summary>
        /// The number of hits required to complete the bash successfully.
        /// </summary>
        public int RequiredHits { get; protected set; }

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            double spinnerRotationRatio = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 3, 5, 7.5);
            RequiredHits = (int)Math.Max(1, Duration / 1000f * spinnerRotationRatio);
        }
    }
}