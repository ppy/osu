// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;
using osu.Game.Beatmaps;
using osu.Game.Online.API;

namespace osu.Desktop.IPC.Messages
{
    public class PlayerStateMessage : OsuWebSocketMessage
    {
        [JsonPropertyName("beatmap")]
        public required Beatmap Beatmap { get; init; }

        [JsonPropertyName("ruleset_id")]
        public required int RulesetId { get; init; }

        [JsonPropertyName("mods")]
        public required APIMod[] Mods { get; init; }
    }

    public class Beatmap
    {
        [JsonPropertyName("beatmap_id")]
        public required int BeatmapId { get; init; }

        [JsonPropertyName("beatmapset_id")]
        public required int BeatmapSetId { get; init; }

        [JsonPropertyName("beatmap_hash")]
        public required string BeatmapHash { get; init; }

        [JsonPropertyName("metadata")]
        public required BeatmapMetadata Metadata { get; init; }

        [JsonPropertyName("difficulty")]
        public required BeatmapDifficulty Difficulty { get; init; }

        [JsonPropertyName("difficulty_name")]
        public required string DifficultyName { get; init; }

        [JsonPropertyName("ruleset_id")]
        public required int RulesetId { get; init; }

        [JsonPropertyName("bpm")]
        public required double BPM { get; init; }

        [JsonPropertyName("star_rating")]
        public required double StarRating { get; init; }

        [JsonPropertyName("max_combo")]
        public required int MaxCombo { get; init; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter<BeatmapOnlineStatus>))]
        public required BeatmapOnlineStatus Status { get; init; }
    }

    public class BeatmapMetadata
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

    public class BeatmapDifficulty
    {
        [JsonPropertyName("approach_rate")]
        public required double ApproachRate { get; init; }

        [JsonPropertyName("circle_size")]
        public required double CircleSize { get; init; }

        [JsonPropertyName("overall_difficulty")]
        public required double OverallDifficulty { get; init; }

        [JsonPropertyName("drain_rate")]
        public required double DrainRate { get; init; }
    }
}
