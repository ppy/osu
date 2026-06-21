// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace osu.Game.IPC.Messages
{
    public class WebSocketMod
    {
        [JsonPropertyName("acronym")]
        public required string Acronym { get; init; }

        [JsonPropertyName("settings")]
        public required Dictionary<string, object> Settings { get; init; }
    }
}
