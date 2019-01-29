// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap metrics based on acculumated online data from community plays.
    /// </summary>
    public class BeatmapMetrics
    {
        /// <summary>
        /// Total vote counts of user ratings on a scale of 0..10 where 0 is unused (probably will be fixed at API?).
        /// </summary>
        public IEnumerable<int> Ratings { get; set; }

        /// <summary>
        /// Points of failure on a relative time scale (usually 0..100).
        /// </summary>
        [JsonProperty(@"fail")]
        public IEnumerable<int> Fails { get; set; }

        /// <summary>
        /// Points of retry on a relative time scale (usually 0..100).
        /// </summary>
        [JsonProperty(@"exit")]
        public IEnumerable<int> Retries { get; set; }
    }
}
