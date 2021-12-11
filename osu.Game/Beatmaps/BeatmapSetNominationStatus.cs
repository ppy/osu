// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Contains information about the current nomination status of a beatmap set.
    /// </summary>
    public class BeatmapSetNominationStatus
    {
        /// <summary>
        /// The current number of nominations that the set has received.
        /// </summary>
        [JsonProperty(@"current")]
        public int Current { get; set; }

        /// <summary>
        /// The number of nominations required so that the map is eligible for qualification.
        /// </summary>
        [JsonProperty(@"required")]
        public int Required { get; set; }
    }
}
