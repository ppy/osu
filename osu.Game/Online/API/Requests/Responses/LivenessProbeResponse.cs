// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class LivenessProbeResponse
    {
        [JsonProperty("status")]
        public LivenessStatus Status { get; set; }

        [JsonProperty("reason")]
        public string? Reason { get; set; }

        public enum LivenessStatus
        {
            [EnumMember(Value = "up")]
            Up,

            [EnumMember(Value = "down")]
            Down,
        }
    }
}
