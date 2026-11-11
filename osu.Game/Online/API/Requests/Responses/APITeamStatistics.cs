// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APITeamStatistics
    {
        [JsonProperty(@"play_count")]
        public int PlayCount;

        [JsonProperty(@"ranked_score")]
        public long RankedScore;

        [JsonProperty(@"performance")]
        public int Performance;

        [JsonProperty(@"rank")]
        public int? Rank;
    }
}
