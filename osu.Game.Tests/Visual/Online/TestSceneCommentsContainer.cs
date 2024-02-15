// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneCommentsContainer : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private CommentsContainer commentsContainer = null!;

        private TextBox editorTextBox = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = commentsContainer = new CommentsContainer()
            };
            editorTextBox = commentsContainer.ChildrenOfType<TextBox>().First();
        });

        [Test]
        public void TestIdleState()
        {
            AddUntilStep("loading spinner shown",
                () => commentsContainer.ChildrenOfType<CommentsShowMoreButton>().Single().IsLoading);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSingleCommentsPage(bool withPinned)
        {
            setUpCommentsResponse(getExampleComments(withPinned));
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddUntilStep("show more button hidden",
                () => commentsContainer.ChildrenOfType<CommentsShowMoreButton>().Single().Alpha == 0);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestMultipleCommentPages(bool withPinned)
        {
            var comments = getExampleComments(withPinned);
            comments.HasMore = true;
            comments.TopLevelCount = 10;

            setUpCommentsResponse(comments);
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddUntilStep("show more button visible",
                () => commentsContainer.ChildrenOfType<CommentsShowMoreButton>().Single().Alpha == 1);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestMultipleLoads(bool withPinned)
        {
            var comments = getExampleComments(withPinned);
            int topLevelCommentCount = comments.Comments.Count;

            AddStep("hide container", () => commentsContainer.Hide());
            setUpCommentsResponse(comments);
            AddRepeatStep("show comments multiple times",
                () => commentsContainer.ShowComments(CommentableType.Beatmapset, 456), 2);
            AddStep("show container", () => commentsContainer.Show());
            AddUntilStep("comment count is correct",
                () => commentsContainer.ChildrenOfType<DrawableComment>().Count() == topLevelCommentCount);
        }

        [Test]
        public void TestNoComment()
        {
            var comments = getExampleComments();
            comments.Comments.Clear();

            setUpCommentsResponse(comments);
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddAssert("no comment shown", () => !commentsContainer.ChildrenOfType<DrawableComment>().Any());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSingleComment(bool withPinned)
        {
            var comment = new Comment
            {
                Id = 1,
                Message = "This is a single comment",
                LegacyName = "SingleUser",
                CreatedAt = DateTimeOffset.Now,
                VotesCount = 0,
                Pinned = withPinned,
            };

            var bundle = new CommentBundle
            {
                Comments = new List<Comment> { comment },
                IncludedComments = new List<Comment>(),
                PinnedComments = new List<Comment>(),
            };

            if (withPinned)
                bundle.PinnedComments.Add(comment);

            setUpCommentsResponse(bundle);
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddUntilStep("wait comment load", () => commentsContainer.ChildrenOfType<DrawableComment>().Any());
            AddAssert("only one comment shown", () =>
                commentsContainer.ChildrenOfType<DrawableComment>().Count(d => d.Comment.Pinned == withPinned) == 1);
        }

        [Test]
        public void TestPost()
        {
            setUpCommentsResponse(new CommentBundle { Comments = new List<Comment>() });
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddAssert("no comments placeholder shown", () => commentsContainer.ChildrenOfType<CommentsContainer.NoCommentsPlaceholder>().Any());

            setUpPostResponse();
            AddStep("enter text", () => editorTextBox.Current.Value = "comm");
            AddStep("submit", () => commentsContainer.ChildrenOfType<RoundedButton>().First().TriggerClick());

            AddUntilStep("comment sent", () =>
            {
                string writtenText = editorTextBox.Current.Value;
                var comment = commentsContainer.ChildrenOfType<DrawableComment>().LastOrDefault();
                return comment != null && comment.ChildrenOfType<SpriteText>().Any(y => y.Text == writtenText);
            });
            AddAssert("no comments placeholder removed", () => !commentsContainer.ChildrenOfType<CommentsContainer.NoCommentsPlaceholder>().Any());
        }

        [Test]
        public void TestPostWithExistingComments()
        {
            setUpCommentsResponse(getExampleComments());
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));

            setUpPostResponse();
            AddStep("enter text", () => editorTextBox.Current.Value = "comm");
            AddStep("submit", () => commentsContainer.ChildrenOfType<CommentEditor>().Single().ChildrenOfType<RoundedButton>().First().TriggerClick());

            AddUntilStep("comment sent", () =>
            {
                string writtenText = editorTextBox.Current.Value;
                var comment = commentsContainer.ChildrenOfType<DrawableComment>().LastOrDefault();
                return comment != null && comment.ChildrenOfType<SpriteText>().Any(y => y.Text == writtenText);
            });
        }

        [Test]
        public void TestPostAsOwner()
        {
            setUpCommentsResponse(getExampleComments());
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));

            setUpPostResponse(true);
            AddStep("enter text", () => editorTextBox.Current.Value = "comm");
            AddStep("submit", () => commentsContainer.ChildrenOfType<CommentEditor>().Single().ChildrenOfType<RoundedButton>().First().TriggerClick());

            AddUntilStep("comment sent", () =>
            {
                string writtenText = editorTextBox.Current.Value;
                var comment = commentsContainer.ChildrenOfType<DrawableComment>().LastOrDefault();
                return comment != null && comment.ChildrenOfType<SpriteText>().Any(y => y.Text == writtenText) && comment.ChildrenOfType<SpriteText>().Any(y => y.Text == "MAPPER");
            });
        }

        private void setUpCommentsResponse(CommentBundle commentBundle)
            => AddStep("set up response", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (!(request is GetCommentsRequest getCommentsRequest))
                        return false;

                    getCommentsRequest.TriggerSuccess(commentBundle);
                    return true;
                };
            });

        private void setUpPostResponse(bool asOwner = false)
            => AddStep("set up response", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (!(request is CommentPostRequest req))
                        return false;

                    var bundle = new CommentBundle
                    {
                        Comments = new List<Comment>
                        {
                            new Comment
                            {
                                Id = 98,
                                Message = req.Message,
                                LegacyName = "FirstUser",
                                CreatedAt = DateTimeOffset.Now,
                                VotesCount = 98,
                                CommentableId = 2001,
                                CommentableType = "test",
                            }
                        }
                    };

                    if (asOwner)
                    {
                        bundle.Comments[0].UserId = 1001;
                        bundle.Comments[0].User = new APIUser { Id = 1001, Username = "FirstUser" };
                        bundle.CommentableMeta.Add(new CommentableMeta
                        {
                            Id = 2001,
                            OwnerId = 1001,
                            OwnerTitle = "MAPPER",
                            Type = "test",
                        });
                    }

                    req.TriggerSuccess(bundle);
                    return true;
                };
            });

        private static CommentBundle getExampleComments(bool withPinned = false)
        {
            var bundle = new CommentBundle
            {
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = 1,
                        Message = "This is a comment",
                        LegacyName = "FirstUser",
                        CreatedAt = DateTimeOffset.Now,
                        VotesCount = 19,
                        RepliesCount = 1
                    },
                    new Comment
                    {
                        Id = 5,
                        ParentId = 1,
                        Message = "This is a child comment",
                        LegacyName = "SecondUser",
                        CreatedAt = DateTimeOffset.Now,
                        VotesCount = 4,
                    },
                    new Comment
                    {
                        Id = 10,
                        Message = "This is another comment",
                        LegacyName = "ThirdUser",
                        CreatedAt = DateTimeOffset.Now,
                        VotesCount = 0
                    },
                },
                IncludedComments = new List<Comment>(),
                PinnedComments = new List<Comment>(),
            };

            if (withPinned)
            {
                var pinnedComment = new Comment
                {
                    Id = 15,
                    Message = "This is pinned comment",
                    LegacyName = "PinnedUser",
                    CreatedAt = DateTimeOffset.Now,
                    VotesCount = 999,
                    Pinned = true,
                    RepliesCount = 1,
                };

                bundle.Comments.Add(pinnedComment);
                bundle.PinnedComments.Add(pinnedComment);

                bundle.Comments.Add(new Comment
                {
                    Id = 20,
                    Message = "Reply to pinned comment",
                    LegacyName = "AbandonedUser",
                    CreatedAt = DateTimeOffset.Now,
                    VotesCount = 0,
                    ParentId = 15,
                });
            }

            return bundle;
        }
    }
}
