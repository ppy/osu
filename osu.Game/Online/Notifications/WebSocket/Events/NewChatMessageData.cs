// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;

namespace osu.Game.Online.Notifications.WebSocket.Events
{
    /// <summary>
    /// A websocket message sent from the server when new messages arrive.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewChatMessageData
    {
        [JsonProperty(@"messages")]
        public List<Message> Messages { get; set; } = null!;

        [JsonProperty(@"users")]
        private List<APIUser> users { get; set; } = null!;

        [OnDeserialized]
        private void onDeserialised(StreamingContext context)
        {
            foreach (var m in Messages)
                m.Sender = users.Single(u => u.OnlineID == m.SenderId);
        }
    }
}
