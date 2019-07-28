// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetMetrics
    {
        /// <summary>
        /// Total vote counts of user ratings on a scale of 0..10 where 0 is unused (probably will be fixed at API?).
        /// </summary>
        [JsonProperty("ratings")]
        public int[] Ratings { get; set; } = Array.Empty<int>();
    }
}
