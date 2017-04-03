// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;

namespace osu.Game.Modes.Taiko.Objects
{
    public class BarLine
    {
        /// <summary>
        /// The start time of the control point this bar line represents.
        /// </summary>
        public double StartTime;

        /// <summary>
        /// The time to scroll in the bar line.
        /// </summary>
        public double PreEmpt;

        public void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            PreEmpt = TaikoHitObject.BASE_SCROLL_TIME / difficulty.SliderMultiplier * timing.BeatLengthAt(StartTime) * timing.SpeedMultiplierAt(StartTime) / 1000;
        }
    }
}
