// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;

namespace osu.Desktop.IPC.Messages
{
    public class UserActivityWebSocketMessage : OsuWebSocketMessage
    {
        [JsonPropertyName("status")]
        public required string Status { get; init; }
    }
}
