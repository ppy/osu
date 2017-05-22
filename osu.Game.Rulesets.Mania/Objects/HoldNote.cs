// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which requires pressing, holding, and releasing a key.
    /// </summary>
    public class HoldNote : Note, IHasEndTime
    {
        /// <summary>
        /// Lenience of release hit windows. This is to make cases where the hold note release
        /// is timed alongside presses of other hit objects less awkward.
        /// </summary>
        private const double release_window_lenience = 1.5;

        public double Duration { get; set; }
        public double EndTime => StartTime + Duration;

        /// <summary>
        /// The key-release hit windows for this hold note.
        /// </summary>
        protected HitWindows ReleaseHitWindows = new HitWindows();

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            ReleaseHitWindows = HitWindows * release_window_lenience;
        }
    }
}
