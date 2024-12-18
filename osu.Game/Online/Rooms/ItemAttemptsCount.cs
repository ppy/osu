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
        [JsonProperty("id")]
        public int PlaylistItemID { get; set; }

        [JsonProperty("attempts")]
        public int Attempts { get; set; }
    }
}
