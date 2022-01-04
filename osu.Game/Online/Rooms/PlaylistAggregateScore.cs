// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// Represents aggregated score for the local user for a playlist.
    /// </summary>
    public class PlaylistAggregateScore
    {
        [JsonProperty("playlist_item_attempts")]
        public ItemAttemptsCount[] PlaylistItemAttempts { get; set; }
    }
}
