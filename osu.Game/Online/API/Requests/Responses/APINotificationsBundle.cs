// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APINotificationsBundle
    {
        [JsonProperty(@"has_more")]
        public bool HasMore { get; set; }

        [JsonProperty(@"notifications")]
        public APINotification[] Notifications { get; set; } = null!;

        [JsonProperty(@"notification_endpoint")]
        public string Endpoint { get; set; } = null!;
    }
}
