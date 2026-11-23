// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISeason
    {
        [JsonProperty(@"id")]
        public int ID;

        [JsonProperty(@"name")]
        public string Name = string.Empty;

        [JsonProperty(@"ruleset_id")]
        public int? RulesetID;

        [JsonProperty(@"variant_id")]
        public int? VariantID;

        [JsonProperty(@"room_count")]
        public int RoomCount;

        [JsonProperty(@"start_date")]
        public DateTimeOffset? StartDate;

        [JsonProperty(@"end_date")]
        public DateTimeOffset? EndDate;
    }
}
