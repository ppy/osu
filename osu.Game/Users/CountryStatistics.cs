// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Users
{
    public class CountryStatistics
    {
        [JsonProperty(@"code")]
        public CountryCode Code;

        [JsonProperty(@"active_users")]
        public long ActiveUsers;

        [JsonProperty(@"play_count")]
        public long PlayCount;

        [JsonProperty(@"ranked_score")]
        public long RankedScore;

        [JsonProperty(@"performance")]
        public long Performance;
    }
}
