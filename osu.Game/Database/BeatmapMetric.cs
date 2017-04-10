// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Database
{
    public class BeatmapMetric
    {
        /// <summary>
        /// Ratings for a beatmap, length should be 10
        /// </summary>
        public int[] Ratings { get; set; }

        /// <summary>
        /// Fails for a beatmap, length should be 100
        /// </summary>
        public int[] Fails { get; set; }

        /// <summary>
        /// Retries for a beatmap, length should be 100
        /// </summary>
        public int[] Retries { get; set; }
    }
}
