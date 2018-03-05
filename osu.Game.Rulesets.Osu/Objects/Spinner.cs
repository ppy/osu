// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Spinner : OsuHitObject, IHasEndTime
    {
        public double EndTime { get; set; }
        public double Duration => EndTime - StartTime;

        /// <summary>
        /// Number of spins required to finish the spinner without miss.
        /// </summary>
        public int SpinsRequired { get; protected set; } = 1;

        public override bool NewCombo => true;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            SpinsRequired = (int)(Duration / 1000 * BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 3, 5, 7.5));

            // spinning doesn't match 1:1 with stable, so let's fudge them easier for the time being.
            SpinsRequired = (int)Math.Max(1, SpinsRequired * 0.6);
        }
    }
}
