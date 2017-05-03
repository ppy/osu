// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class HoldNote : Note, IHasEndTime
    {
        /// <summary>
        /// Lenience of release hit windows.
        /// </summary>
        private const double release_window_lenience = 1.5;

        public double Duration { get; set; }
        public double EndTime => StartTime + Duration;

        public HitWindows ReleaseHitWindows { get; protected set; } = new HitWindows();

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            ReleaseHitWindows = HitWindows * release_window_lenience;
        }
    }
}
