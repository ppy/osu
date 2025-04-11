// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// Represents attempts on a specific playlist item.
    /// </summary>
    public class ItemAttemptsCount
    {
        /// <summary>
        /// The playlist item this object describes.
        /// </summary>
        [JsonProperty("id")]
        public int PlaylistItemID { get; set; }

        /// <summary>
        /// The number of times the user attempted the playlist item.
        /// </summary>
        [JsonProperty("attempts")]
        public int Attempts { get; set; }

        /// <summary>
        /// Whether the user has a passing score on the playlist item.
        /// </summary>
        [JsonProperty("passed")]
        public bool Passed { get; set; }
    }
}
