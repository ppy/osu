// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace osu.Game.IPC.Models
{
    public class WebSocketWatchingReplayUserActivityData
    {
        [JsonPropertyName("score_id")]
        public required long ScoreId { get; init; }

        [JsonPropertyName("player_name")]
        public required string PlayerName { get; init; }

        [JsonPropertyName("beatmap_id")]
        public required int BeatmapId { get; init; }
    }
}
