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
        public string? FlagUrl { get; set; }

        [JsonProperty(@"cover_url")]
        public string? CoverUrl { get; set; }

        [JsonProperty(@"default_ruleset_id")]
        public int DefaultRulesetId { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty(@"description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(@"leader")]
        public APIUser Leader { get; set; } = new APIUser();

        [JsonProperty(@"is_open")]
        public bool IsOpen { get; set; }

        [JsonProperty(@"members_count")]
        public int MembersCount { get; set; }

        [JsonProperty(@"empty_slots")]
        public int EmptySlots { get; set; }

        [JsonProperty(@"statistics")]
        public APITeamStatistics Statistics { get; set; } = new APITeamStatistics();

        public int OnlineID => Id;
    }

    public class APITeamStatistics
    {
        [JsonProperty(@"play_count")]
        public int PlayCount { get; set; }

        [JsonProperty(@"ranked_score")]
        public long RankedScore { get; set; }

        [JsonProperty(@"performance")]
        public int Performance { get; set; }

        [JsonProperty(@"rank")]
        public int? Rank { get; set; }
    }
}
