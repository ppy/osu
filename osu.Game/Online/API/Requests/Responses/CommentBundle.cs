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
                comments.ForEach(comment =>
                {
                    if (comment.IsReply)
                        comments.ForEach(parent => checkRepliesDependency(parent, comment));
                });
            }
        }

        [JsonProperty(@"has_more")]
        public bool HasMore { get; set; }

        [JsonProperty(@"has_more_id")]
        public long? HasMoreId { get; set; }

        [JsonProperty(@"user_follow")]
        public bool UserFollow { get; set; }

        private List<Comment> includedComments;

        [JsonProperty(@"included_comments")]
        public List<Comment> IncludedComments
        {
            get => includedComments;
            set
            {
                includedComments = value;
                value.ForEach(comment =>
                {
                    if (comment.IsReply)
                    {
                        value.ForEach(parent => checkRepliesDependency(parent, comment));
                        comments.ForEach(parent => checkRepliesDependency(parent, comment));
                    }
                });
            }
        }

        [JsonProperty(@"user_votes")]
        private List<long> userVotes
        {
            set
            {
                value.ForEach(v =>
                {
                    Comments.ForEach(c => checkVotesDependency(v, c));
                    IncludedComments.ForEach(c => checkVotesDependency(v, c));
                });
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
                    Comments.ForEach(c => checkUserCommentDependency(u, c));
                    IncludedComments.ForEach(c => checkUserCommentDependency(u, c));
                });
            }
        }

        [JsonProperty(@"total")]
        public int Total { get; set; }

        [JsonProperty(@"top_level_count")]
        public int TopLevelCount { get; set; }

        private void checkRepliesDependency(Comment parent, Comment reply)
        {
            if (parent.Id == reply.ParentId)
            {
                parent.Replies.Add(reply);
                reply.ParentComment = parent;
            }
        }

        private void checkVotesDependency(long votedCommentId, Comment comment)
        {
            if (votedCommentId == comment.Id)
                comment.IsVoted = true;
        }

        private void checkUserCommentDependency(User user, Comment comment)
        {
            if (comment.UserId == user.Id)
                comment.User = user;

            if (comment.EditedById == user.Id)
                comment.EditedUser = user;
        }
    }
}
