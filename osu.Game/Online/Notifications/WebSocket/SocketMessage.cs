// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.Online.Notifications.WebSocket
{
    /// <summary>
    /// A websocket message, sent either from the client or server.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SocketMessage
    {
        [JsonProperty(@"event")]
        public string Event { get; set; } = null!;

        [JsonProperty(@"data")]
        public JObject? Data { get; set; }

        [JsonProperty(@"error")]
        public string? Error { get; set; }
    }
}
