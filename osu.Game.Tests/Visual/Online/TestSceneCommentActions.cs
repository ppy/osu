// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneCommentActions : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Cached]
        private readonly DialogOverlay dialogOverlay = new DialogOverlay();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private CommentsContainer commentsContainer = null!;

        [SetUpSteps]
        public void SetUp()
        {
            API.Login("test", "test");
            Schedule(() =>
            {
                if (dialogOverlay.Parent != null) Remove(dialogOverlay, false);
                Children = new Container<Drawable>[]
                {
                    new BasicScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = commentsContainer = new CommentsContainer()
                    },
                    dialogOverlay
                };
            });
        }

        [Test]
        public void TestNonOwnCommentCantBeDeleted()
        {
            addTestComments();
        }

        [Test]
        public void TestDeletion()
        {
            addTestComments();
        }

        private void addTestComments()
        {
            CommentBundle cb = new CommentBundle
            {
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = 1,
                        Message = "This is our comment",
                        UserId = API.LocalUser.Value.Id,
                        CreatedAt = DateTimeOffset.Now,
                        User = API.LocalUser.Value,
                    },
                    new Comment
                    {
                        Id = 2,
                        Message = "This is a comment by another user",
                        UserId = API.LocalUser.Value.Id + 1,
                        CreatedAt = DateTimeOffset.Now,
                        User = new APIUser
                        {
                            Id = API.LocalUser.Value.Id + 1,
                            Username = "Another user"
                        }
                    },
                },
                IncludedComments = new List<Comment>(),
                PinnedComments = new List<Comment>(),
            };
            setUpCommentsResponse(cb);
            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
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
    }
}
