// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko.Objects
{
    public class BarLine
    {
        /// <summary>
        /// The start time of the bar line.
        /// </summary>
        public double StartTime;

        /// <summary>
        /// The time to scroll in the bar line.
        /// </summary>
        public double PreEmpt;

        /// <summary>
        /// If this is a major bar line.
        /// </summary>
        public bool IsMajor;

        public void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            PreEmpt = 600 / beatmap.SliderVelocityAt(StartTime) * 1000;
        }
    }
}
