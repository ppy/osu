// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISeasonStatistics
    {
        [JsonProperty(@"rank")]
        public int Rank;

        [JsonProperty(@"total_score")]
        public long TotalScore;

        [JsonProperty(@"season")]
        public APISeason Season = new APISeason();

        [JsonProperty(@"division")]
        public APISeasonDivision Division = new APISeasonDivision();
    }
}
