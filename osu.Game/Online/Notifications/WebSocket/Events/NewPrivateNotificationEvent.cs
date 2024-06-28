// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.Online.Notifications.WebSocket.Events
{
    /// <summary>
    /// Reference: https://github.com/ppy/osu-web/blob/master/app/Events/NewPrivateNotificationEvent.php
    /// </summary>
    public class NewPrivateNotificationEvent
    {
        [JsonProperty("id")]
        public ulong ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("object_type")]
        public string ObjectType { get; set; } = string.Empty;

        [JsonProperty("object_id")]
        public ulong ObjectId { get; set; }

        [JsonProperty("source_user_id")]
        public uint SourceUserID { get; set; }

        [JsonProperty("is_read")]
        public bool IsRead { get; set; }

        [JsonProperty("details")]
        public JObject? Details { get; set; }
    }
}
