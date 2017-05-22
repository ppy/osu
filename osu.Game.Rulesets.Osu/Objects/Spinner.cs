// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;

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

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            SpinsRequired = (int)(Duration / 1000 * BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 3, 5, 7.5));
        }
    }
}
