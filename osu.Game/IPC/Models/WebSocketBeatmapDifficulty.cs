// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace osu.Game.IPC.Models
{
    public class WebSocketBeatmapDifficulty
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
