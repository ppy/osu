// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Database;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class HoldNoteTail : Note
    {
        /// <summary>
        /// Lenience of release hit windows. This is to make cases where the hold note release
        /// is timed alongside presses of other hit objects less awkward.
        /// </summary>
        private const double release_window_lenience = 1.5;

        public override void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(controlPointInfo, difficulty);

            HitWindows *= release_window_lenience;
        }
    }
}