// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Desktop.IPC.Messages
{
    public class UserActivityMessage : OsuWebSocketMessage
    {
        [JsonProperty("status")]
        public required string Status { get; init; }
    }
}
