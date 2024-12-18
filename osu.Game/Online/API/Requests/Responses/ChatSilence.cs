// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ChatSilence
    {
        [JsonProperty("id")]
        public uint Id { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }
}
