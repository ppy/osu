// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Contains information about the current hype status of a beatmap set.
    /// </summary>
    public class BeatmapSetHypeStatus
    {
        /// <summary>
        /// The current number of hypes that the set has received.
        /// </summary>
        [JsonProperty(@"current")]
        public int Current { get; set; }

        /// <summary>
        /// The number of hypes required so that the set is eligible for nomination.
        /// </summary>
        [JsonProperty(@"required")]
        public int Required { get; set; }
    }
}
