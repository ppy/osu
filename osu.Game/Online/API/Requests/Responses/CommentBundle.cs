// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Online.API.Requests.Responses
{
    public class CommentBundle
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

        [JsonProperty(@"pinned_comments")]
        public List<Comment> PinnedComments { get; set; }

        private List<long> userVotes;

        [JsonProperty(@"user_votes")]
        public List<long> UserVotes
        {
            get => userVotes;
            set
            {
                userVotes = value;

                Comments.ForEach(c => c.IsVoted = value.Contains(c.Id));
                IncludedComments.ForEach(c => c.IsVoted = value.Contains(c.Id));
            }
        }

        private List<APIUser> users;

        [JsonProperty(@"users")]
        public List<APIUser> Users
        {
            get => users;
            set
            {
                users = value;

                foreach (var user in value)
                {
                    foreach (var comment in Comments.Concat(IncludedComments).Concat(PinnedComments))
                    {
                        if (comment.UserId == user.Id)
                            comment.User = user;

                        if (comment.EditedById == user.Id)
                            comment.EditedUser = user;
                    }
                }
            }
        }

        [JsonProperty(@"total")]
        public int Total { get; set; }

        [JsonProperty(@"top_level_count")]
        public int TopLevelCount { get; set; }
    }
}
