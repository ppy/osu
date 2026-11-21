// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserMatchmakingRatingHistory
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt;

        [JsonProperty("room_id")]
        public int RoomId;

        [JsonProperty("result")]
        public MatchResult Result;

        [JsonProperty("elo_after")]
        public int RatingAfter;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum MatchResult
        {
            Loss,
            Win,
        }
    }
}
