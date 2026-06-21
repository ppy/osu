// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace osu.Game.IPC.Messages
{
    public class WebSocketInLobbyUserActivityData
    {
        [JsonPropertyName("room_id")]
        public required long RoomId { get; init; }

        [JsonPropertyName("room_name")]
        public required string RoomName { get; init; }
    }
}
