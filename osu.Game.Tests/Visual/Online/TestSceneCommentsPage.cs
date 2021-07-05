// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays.Comments;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osuTK;
using JetBrains.Annotations;
using NUnit.Framework;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneCommentsPage : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly BindableBool showDeleted = new BindableBool();
        private readonly Container content;

        private TestCommentsPage commentsPage;

        public TestSceneCommentsPage()
        {
            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Y,
                        Width = 200,
                        Child = new OsuCheckbox
                        {
                            Current = showDeleted,
                            LabelText = @"Show Deleted"
                        }
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    }
                }
            });
        }

        [Test]
        public void TestAppendDuplicatedComment()
        {
            AddStep("Create page", () => createPage(getCommentBundle()));
            AddAssert("Dictionary length is 10", () => commentsPage?.DictionaryLength == 10);
            AddStep("Append existing comment", () => commentsPage?.AppendComments(getCommentSubBundle()));
            AddAssert("Dictionary length is 10", () => commentsPage?.DictionaryLength == 10);
        }

        [Test]
        public void TestEmptyBundle()
        {
            AddStep("Create page", () => createPage(getEmptyCommentBundle()));
            AddAssert("Dictionary length is 0", () => commentsPage?.DictionaryLength == 0);
        }

        private void createPage(CommentBundle commentBundle)
        {
            commentsPage = null;
            content.Clear();
            content.Add(commentsPage = new TestCommentsPage(commentBundle)
            {
                ShowDeleted = { BindTarget = showDeleted }
            });
        }

        private CommentBundle getEmptyCommentBundle() => new CommentBundle
        {
            Comments = new List<Comment>(),
        };

        private CommentBundle getCommentBundle() => new CommentBundle
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
                    Id = 100,
                    Message = "This comment has \"load replies\" button because it has unloaded replies",
                    LegacyName = "TestUser1100",
                    CreatedAt = DateTimeOffset.Now,
                    VotesCount = 5,
                    RepliesCount = 2,
                },
                new Comment
                {
                    Id = 111,
                    Message = "This comment has \"Show More\" button because it has unloaded replies, but some of them are loaded",
                    LegacyName = "TestUser1111",
                    CreatedAt = DateTimeOffset.Now,
                    VotesCount = 100,
                    RepliesCount = 2,
                },
                new Comment
                {
                    Id = 112,
                    ParentId = 111,
                    Message = "I'm here to make my parent work",
                    LegacyName = "someone",
                    CreatedAt = DateTimeOffset.Now,
                    VotesCount = 2,
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
                    VotesCount = 1000,
                    EditedById = 1,
                }
            },
            IncludedComments = new List<Comment>(),
            UserVotes = new List<long>
            {
                5
            },
            Users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "Good_Admin"
                }
            },
        };

        private CommentBundle getCommentSubBundle() => new CommentBundle
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
            },
            IncludedComments = new List<Comment>(),
        };

        private class TestCommentsPage : CommentsPage
        {
            public TestCommentsPage(CommentBundle commentBundle)
                : base(commentBundle)
            {
            }

            public new void AppendComments([NotNull] CommentBundle bundle) => base.AppendComments(bundle);

            public int DictionaryLength => CommentDictionary.Count;
        }
    }
}
