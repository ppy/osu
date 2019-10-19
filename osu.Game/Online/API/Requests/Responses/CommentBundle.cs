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
                        comments.ForEach(parent => checkParentChildDependency(parent, child));
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
                value.ForEach(child =>
                {
                    if (child.ParentId != null)
                    {
                        value.ForEach(parent => checkParentChildDependency(parent, child));
                        comments.ForEach(parent => checkParentChildDependency(parent, child));
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

        private void checkParentChildDependency(Comment parent, Comment child)
        {
            if (parent.Id == child.ParentId)
            {
                parent.ChildComments.Add(child);
                child.ParentComment = parent;
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
