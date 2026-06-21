// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace osu.Game.IPC.Messages
{
    public class PlayerStateWebSocketMessage : OsuWebSocketMessage
    {
        [JsonPropertyName("beatmap")]
        public required WebSocketBeatmap Beatmap { get; init; }

        [JsonPropertyName("ruleset_id")]
        public required int RulesetId { get; init; }

        [JsonPropertyName("mods")]
        public required WebSocketMod[] Mods { get; init; }
    }
}
