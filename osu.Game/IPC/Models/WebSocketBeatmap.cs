// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;
using osu.Game.Beatmaps;

namespace osu.Game.IPC.Models
{
    public class WebSocketBeatmap
    {
        [JsonPropertyName("beatmap_id")]
        public required int BeatmapId { get; init; }

        [JsonPropertyName("beatmapset_id")]
        public required int BeatmapSetId { get; init; }

        [JsonPropertyName("beatmap_hash")]
        public required string BeatmapHash { get; init; }

        [JsonPropertyName("metadata")]
        public required WebSocketBeatmapMetadata Metadata { get; init; }

        [JsonPropertyName("difficulty")]
        public required WebSocketBeatmapDifficulty Difficulty { get; init; }

        [JsonPropertyName("difficulty_name")]
        public required string DifficultyName { get; init; }

        [JsonPropertyName("ruleset_id")]
        public required int RulesetId { get; init; }

        [JsonPropertyName("bpm")]
        public required double BPM { get; init; }

        [JsonPropertyName("star_rating")]
        public required double StarRating { get; init; }

        [JsonPropertyName("maximum_pp")]
        public required double MaximumPP { get; init; }

        [JsonPropertyName("max_combo")]
        public required int MaxCombo { get; init; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter<BeatmapOnlineStatus>))]
        public required BeatmapOnlineStatus Status { get; init; }

        [JsonPropertyName("total_length")]
        public required int TotalLength { get; init; }

        [JsonPropertyName("drain_length")]
        public required int DrainLength { get; init; }
    }
}
