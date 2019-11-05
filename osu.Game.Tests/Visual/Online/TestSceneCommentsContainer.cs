// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneCommentsContainer : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CommentsContainer),
            typeof(CommentsHeader),
            typeof(DrawableComment),
            typeof(HeaderButton),
            typeof(SortTabControl),
            typeof(ShowChildrenButton),
            typeof(DeletedChildrenPlaceholder),
            typeof(VotePill)
        };

        protected override bool UseOnlineAPI => true;

        public TestSceneCommentsContainer()
        {
            CommentsContainer commentsContainer = new CommentsContainer();
            BasicScrollContainer scroll;

            Add(scroll = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = commentsContainer
            });

            AddStep("Idle state", () =>
            {
                scroll.Clear();
                scroll.Add(commentsContainer = new CommentsContainer());
            });
            AddStep("Big Black comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 41823));
            AddStep("Airman comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 24313));
            AddStep("lazer build comments", () => commentsContainer.ShowComments(CommentableType.Build, 4772));
            AddStep("local comments", () => commentsContainer.ShowComments(comment_bundle));
            AddStep("local empty comments", () => commentsContainer.ShowComments(empty_comment_bundle));
        }

        private static readonly CommentBundle empty_comment_bundle = new CommentBundle
        {
            Comments = new List<Comment>(),
        };

        private static readonly CommentBundle comment_bundle = new CommentBundle
        {
            Comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Message = "Simple test comment",
                    LegacyName = "TestUser1",
                    CreatedAt = DateTimeOffset.Now,
                    VotesCount = 5
                },
                new Comment
                {
                    Id = 2,
                    Message = "This comment has been deleted :( but visible for admins",
                    LegacyName = "TestUser2",
                    CreatedAt = DateTimeOffset.Now,
                    DeletedAt = DateTimeOffset.Now,
                    VotesCount = 5
                },
                new Comment
                {
                    Id = 3,
                    Message = "This comment is a top level",
                    LegacyName = "TestUser3",
                    CreatedAt = DateTimeOffset.Now,
                    RepliesCount = 2,
                },
                new Comment
                {
                    Id = 4,
                    ParentId = 3,
                    Message = "And this is a reply",
                    RepliesCount = 1,
                    LegacyName = "TestUser1",
                    CreatedAt = DateTimeOffset.Now,
                },
                new Comment
                {
                    Id = 15,
                    ParentId = 4,
                    Message = "Reply to reply",
                    LegacyName = "TestUser1",
                    CreatedAt = DateTimeOffset.Now,
                },
                new Comment
                {
                    Id = 6,
                    ParentId = 3,
                    LegacyName = "TestUser11515",
                    CreatedAt = DateTimeOffset.Now,
                    DeletedAt = DateTimeOffset.Now,
                },
                new Comment
                {
                    Id = 5,
                    Message = "This comment is voted and edited",
                    LegacyName = "BigBrainUser",
                    CreatedAt = DateTimeOffset.Now,
                    EditedAt = DateTimeOffset.Now,
                    IsVoted = true,
                    VotesCount = 1000,
                    EditedById = 1,
                }
            },
            IncludedComments = new List<Comment>(),
            Users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "Good_Admin"
                }
            },
            TopLevelCount = 4,
        };
    }
}
