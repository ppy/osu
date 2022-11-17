// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.Online.API.Requests.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APINotification
    {
        [JsonProperty(@"id")]
        public long Id { get; set; }

        [JsonProperty(@"name")]
        public string Name { get; set; } = null!;

        [JsonProperty(@"created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty(@"object_type")]
        public string ObjectType { get; set; } = null!;

        [JsonProperty(@"object_id")]
        public string ObjectId { get; set; } = null!;

        [JsonProperty(@"source_user_id")]
        public long? SourceUserId { get; set; }

        [JsonProperty(@"is_read")]
        public bool IsRead { get; set; }

        [JsonProperty(@"details")]
        public JObject? Details { get; set; }
    }
}
