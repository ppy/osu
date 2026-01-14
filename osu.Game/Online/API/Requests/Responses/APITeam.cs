// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Teams;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APITeam : ITeam, IHasCover
    {
        [JsonProperty(@"id")]
        public int Id { get; set; } = 1;

        [JsonProperty(@"name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(@"short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty(@"flag_url")]
        public string? FlagUrl;

        [JsonProperty(@"cover_url")]
        public string? CoverUrl { get; set; }

        [JsonProperty(@"default_ruleset_id")]
        public int DefaultRulesetId;

        [JsonProperty(@"created_at")]
        public DateTimeOffset CreatedAt;

        [JsonProperty(@"description")]
        public string Description = string.Empty;

        [JsonProperty(@"leader")]
        public APIUser Leader = new APIUser();

        [JsonProperty(@"members")]
        public APIUser[] Members = [];

        [JsonProperty(@"is_open")]
        public bool IsOpen;

        [JsonProperty(@"url")]
        public string? Url;

        [JsonProperty(@"empty_slots")]
        public int EmptySlots;

        [JsonProperty(@"statistics")]
        public APITeamStatistics Statistics = new APITeamStatistics();

        public int OnlineID => Id;
    }

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
