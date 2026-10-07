// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Online.RankedPlay;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIRankedPlayDivision
    {
        [JsonProperty(@"key")]
        public string Key = string.Empty;

        [JsonProperty(@"tier")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RankingTier Tier;

        [JsonProperty(@"division")]
        public Division Division;

        [JsonProperty(@"display_name")]
        public string DisplayName = string.Empty;

        [JsonProperty(@"start_rating")]
        public int StartRating;

        [JsonProperty("@end_rating")]
        public int EndRating;

        public int Width => EndRating - StartRating + 1;
    }
}
