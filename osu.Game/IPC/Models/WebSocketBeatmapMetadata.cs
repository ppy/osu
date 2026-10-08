// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace osu.Game.IPC.Models
{
    public class WebSocketBeatmapMetadata
    {
        [JsonPropertyName("artist")]
        public required string Artist { get; init; }

        [JsonPropertyName("artist_unicode")]
        public required string ArtistUnicode { get; init; }

        [JsonPropertyName("title")]
        public required string Title { get; init; }

        [JsonPropertyName("title_unicode")]
        public required string TitleUnicode { get; init; }

        [JsonPropertyName("author")]
        public required string Author { get; init; }

        [JsonPropertyName("source")]
        public required string Source { get; init; }

        [JsonPropertyName("tags")]
        public required string Tags { get; init; }

        [JsonPropertyName("user_tags")]
        public required string[] UserTags { get; init; }
    }
}
