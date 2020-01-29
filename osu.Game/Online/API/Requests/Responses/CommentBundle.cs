// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Users;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests.Responses
{
    public class CommentBundle
    {
        private List<Comment> comments;

        [JsonProperty(@"comments")]
        public List<Comment> Comments
        {
            get => comments;
            set
            {
                comments = value;
                comments.ForEach(child =>
                {
                    if (child.ParentId != null)
                    {
                        comments.ForEach(parent =>
                        {
                            if (parent.Id == child.ParentId)
                            {
                                parent.ChildComments.Add(child);
                                child.ParentComment = parent;
                            }
                        });
                    }
                });
            }
        }

        [JsonProperty(@"has_more")]
        public bool HasMore { get; set; }

        [JsonProperty(@"has_more_id")]
        public long? HasMoreId { get; set; }

        [JsonProperty(@"user_follow")]
        public bool UserFollow { get; set; }

        [JsonProperty(@"included_comments")]
        public List<Comment> IncludedComments { get; set; }

        private List<long> userVotes;

        [JsonProperty(@"user_votes")]
        public List<long> UserVotes
        {
            get => userVotes;
            set
            {
                userVotes = value;

                Comments.ForEach(c => c.IsVoted = value.Contains(c.Id));
            }
        }

        private List<User> users;

        [JsonProperty(@"users")]
        public List<User> Users
        {
            get => users;
            set
            {
                users = value;

                value.ForEach(u =>
                {
                    Comments.ForEach(c =>
                    {
                        if (c.UserId == u.Id)
                            c.User = u;

                        if (c.EditedById == u.Id)
                            c.EditedUser = u;
                    });
                });
            }
        }

        [JsonProperty(@"total")]
        public int Total { get; set; }

        [JsonProperty(@"top_level_count")]
        public int TopLevelCount { get; set; }
    }
}
