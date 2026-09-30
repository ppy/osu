// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserMatchmakingStatistics
    {
        [JsonProperty("user_id")]
        public int UserId;

        [JsonProperty("pool_id")]
        public int PoolId { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("rank")]
        public int? Rank { get; set; }

        [JsonProperty("plays")]
        public int Plays { get; set; }

        [JsonProperty("total_points")]
        public int TotalPoints { get; set; }

        [JsonProperty("first_placements")]
        public int FirstPlacements { get; set; }

        [JsonProperty("is_rating_provisional")]
        public bool IsRatingProvisional { get; set; }

        [JsonProperty("pool")]
        public APIMatchmakingPool Pool { get; set; } = new APIMatchmakingPool();
    }
}
