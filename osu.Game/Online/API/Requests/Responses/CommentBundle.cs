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
                foreach (var comment in comments)
                {
                    if (comment.IsReply)
                        foreach (var parent in comments)
                        {
                            if (comment.Id != parent.Id)
                                checkParentDependency(parent, comment);

                            if (comment.ParentComment != null)
                                break;
                        }
                }
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

                foreach (var comment in includedComments)
                {
                    if (comment.IsReply)
                    {
                        foreach (var parent in includedComments)
                        {
                            if (comment.Id != parent.Id)
                                checkParentDependency(parent, comment);

                            if (comment.ParentComment != null)
                                break;
                        }

                        if (comment.ParentComment != null)
                        {
                            foreach (var parent in comments)
                            {
                                if (comment.Id != parent.Id)
                                    checkParentDependency(parent, comment);

                                if (comment.ParentComment != null)
                                    break;
                            }
                        }
                    }

                    // The comment can be a parent for comments in Comments
                    foreach (var reply in comments)
                        checkParentDependency(comment, reply);
                }
            }
        }

        [JsonProperty(@"user_votes")]
        private List<long> userVotes
        {
            set
            {
                foreach (var votedCommentId in value)
                {
                    bool voteFinded = false;

                    foreach (var comment in comments)
                    {
                        voteFinded = checkVotesDependency(votedCommentId, comment);

                        if (voteFinded)
                            break;
                    }

                    if (!voteFinded)
                    {
                        foreach (var comment in includedComments)
                        {
                            voteFinded = checkVotesDependency(votedCommentId, comment);

                            if (voteFinded)
                                break;
                        }
                    }
                }
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

        private void checkParentDependency(Comment parent, Comment reply)
        {
            if (parent.Id == reply.ParentId)
            {
                parent.Replies.Add(reply);
                reply.ParentComment = parent;
            }
        }

        private bool checkVotesDependency(long votedCommentId, Comment comment)
        {
            if (votedCommentId == comment.Id)
            {
                comment.IsVoted = true;
                return true;
            }

            return false;
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
