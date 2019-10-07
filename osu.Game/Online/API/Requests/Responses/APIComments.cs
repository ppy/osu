// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Users;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIComments
    {
        [JsonProperty(@"comments")]
        public List<Comment> Comments { get; set; }

        [JsonProperty(@"has_more")]
        public bool HasMore { get; set; }

        [JsonProperty(@"has_more_id")]
        public long? HasMoreId { get; set; }

        [JsonProperty(@"user_follow")]
        public bool UserFollow { get; set; }

        [JsonProperty(@"included_comments")]
        public List<Comment> IncludedComments { get; set; }

        [JsonProperty(@"users")]
        public List<User> Users { get; set; }

        [JsonProperty(@"total")]
        public int Total { get; set; }

        [JsonProperty(@"top_level_count")]
        public int TopLevelCount { get; set; }
    }
}
