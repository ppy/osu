// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIRelation
    {
        [JsonProperty("target_id")]
        public int TargetID { get; set; }

        [JsonProperty("relation_type")]
        public RelationType RelationType { get; set; }

        [JsonProperty("mutual")]
        public bool Mutual { get; set; }

        [JsonProperty("target")]
        public APIUser? TargetUser { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RelationType
    {
        Friend,
        Block,
    }
}
