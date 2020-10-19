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
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneCommentsContainer : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private CommentsContainer commentsContainer;

        [SetUp]
        public void SetUp() => Schedule(() =>
            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = commentsContainer = new CommentsContainer()
            });

        [Test]
        public void TestIdleState()
        {
            AddUntilStep("loading spinner shown",
                () => commentsContainer.ChildrenOfType<CommentsShowMoreButton>().Single().IsLoading);
        }

        [Test]
        public void TestSingleCommentsPage()
        {
            setUpCommentsResponse(exampleComments);
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddUntilStep("show more button hidden",
                () => commentsContainer.ChildrenOfType<CommentsShowMoreButton>().Single().Alpha == 0);
        }

        [Test]
        public void TestMultipleCommentPages()
        {
            var comments = exampleComments;
            comments.HasMore = true;
            comments.TopLevelCount = 10;

            setUpCommentsResponse(comments);
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
            AddUntilStep("show more button visible",
                () => commentsContainer.ChildrenOfType<CommentsShowMoreButton>().Single().Alpha == 1);
        }

        [Test]
        public void TestMultipleLoads()
        {
            var comments = exampleComments;
            int topLevelCommentCount = exampleComments.Comments.Count;

            AddStep("hide container", () => commentsContainer.Hide());
            setUpCommentsResponse(comments);
            AddRepeatStep("show comments multiple times",
                () => commentsContainer.ShowComments(CommentableType.Beatmapset, 456), 2);
            AddStep("show container", () => commentsContainer.Show());
            AddUntilStep("comment count is correct",
                () => commentsContainer.ChildrenOfType<DrawableComment>().Count() == topLevelCommentCount);
        }

        private void setUpCommentsResponse(CommentBundle commentBundle)
            => AddStep("set up response", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (!(request is GetCommentsRequest getCommentsRequest))
                        return;

                    getCommentsRequest.TriggerSuccess(commentBundle);
                };
            });

        private CommentBundle exampleComments => new CommentBundle
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
        };
    }
}
