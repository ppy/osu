// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Users;
using System.Collections.Generic;
using System.Linq;

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

                comments.ForEach(comment =>
                {
                    if (comment.IsReply)
                    {
                        var parent = comments.FirstOrDefault(parent => parent.Id == comment.ParentId);

                        if (parent != null)
                        {
                            parent.Replies.Add(comment);
                            comment.ParentComment = parent;
                        }
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
                userVotes.ForEach(vote => Comments.FirstOrDefault(comment => vote == comment.Id).IsVoted = true);
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

                Comments.ForEach(comment =>
                {
                    comment.User = users.FirstOrDefault(user => user.Id == comment.UserId);

                    if (comment.IsEdited)
                        comment.EditedUser = users.FirstOrDefault(user => user.Id == comment.EditedById);
                });
            }
        }

        [JsonProperty(@"total")]
        public int Total { get; set; }

        [JsonProperty(@"top_level_count")]
        public int TopLevelCount { get; set; }
    }
}
