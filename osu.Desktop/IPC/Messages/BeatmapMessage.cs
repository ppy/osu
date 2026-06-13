// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Beatmaps;

namespace osu.Desktop.IPC.Messages
{
    public class BeatmapMessage : OsuWebSocketMessage
    {
        [JsonProperty("beatmap_id")]
        public required int BeatmapId { get; init; }

        [JsonProperty("beatmap_set_id")]
        public required int BeatmapSetId { get; init; }

        [JsonProperty("beatmap_hash")]
        public required string BeatmapHash { get; init; }

        [JsonProperty("metadata")]
        public required BeatmapMetadata Metadata { get; init; }

        [JsonProperty("difficulty")]
        public required BeatmapDifficulty Difficulty { get; init; }

        [JsonProperty("difficulty_name")]
        public required string DifficultyName { get; init; }

        [JsonProperty("ruleset_id")]
        public required int RulesetId { get; init; }

        [JsonProperty("bpm")]
        public required double BPM { get; init; }

        [JsonProperty("star_rating")]
        public required double StarRating { get; init; }

        [JsonProperty("status")]
        public required BeatmapOnlineStatus Status { get; init; }
    }

    public class BeatmapMetadata
    {
        [JsonProperty("artist")]
        public required string Artist { get; init; }

        [JsonProperty("artist_unicode")]
        public required string ArtistUnicode { get; init; }

        [JsonProperty("title")]
        public required string Title { get; init; }

        [JsonProperty("title_unicode")]
        public required string TitleUnicode { get; init; }

        [JsonProperty("author")]
        public required string Author { get; init; }

        [JsonProperty("source")]
        public required string Source { get; init; }

        [JsonProperty("tags")]
        public required string Tags { get; init; }

        [JsonProperty("user_tags")]
        public required string[] UserTags { get; init; }
    }

    public class BeatmapDifficulty
    {
        [JsonProperty("approach_rate")]
        public required double ApproachRate { get; init; }

        [JsonProperty("circle_size")]
        public required double CircleSize { get; init; }

        [JsonProperty("overall_difficulty")]
        public required double OverallDifficulty { get; init; }

        [JsonProperty("drain_rate")]
        public required double DrainRate { get; init; }
    }
}
