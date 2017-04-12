// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Database
{
    /// <summary>
    /// Beatmap metrics based on acculumated online data from community plays.
    /// </summary>
    public class BeatmapMetrics
    {
        /// <summary>
        /// Total vote counts of user ratings on a scale of 0..length.
        /// </summary>
        public IEnumerable<int> Ratings { get; set; }

        /// <summary>
        /// Points of failure on a relative time scale (usually 0..100).
        /// </summary>
        public IEnumerable<int> Fails { get; set; }

        /// <summary>
        /// Points of retry on a relative time scale (usually 0..100).
        /// </summary>
        public IEnumerable<int> Retries { get; set; }
    }
}
