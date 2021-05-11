// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap metrics based on accumulated online data from community plays.
    /// </summary>
    public class BeatmapMetrics
    {
        /// <summary>
        /// Points of failure on a relative time scale (usually 0..100).
        /// </summary>
        [JsonProperty(@"fail")]
        public int[] Fails { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Points of retry on a relative time scale (usually 0..100).
        /// </summary>
        [JsonProperty(@"exit")]
        public int[] Retries { get; set; } = Array.Empty<int>();
    }
}
