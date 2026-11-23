// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISeasonDivision
    {
        [JsonProperty(@"id")]
        public int ID;

        [JsonProperty(@"name")]
        public string Name = string.Empty;

        [JsonProperty(@"colour_tier")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RankingTier Tier;

        [JsonProperty(@"image@2x_url")]
        public string ImageUrl = string.Empty;

        [JsonProperty(@"threshold")]
        public float Threshold;
    }
}
