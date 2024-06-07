// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ChatAckResponse
    {
        [JsonProperty("silences")]
        public ChatSilence[] Silences { get; set; } = Array.Empty<ChatSilence>();
    }
}
