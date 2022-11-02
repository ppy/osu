// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneCommentActions : OsuManualInputManagerTestScene
    {
        private Container<Drawable> content = null!;
        protected override Container<Drawable> Content => content;
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Cached(typeof(IDialogOverlay))]
        private readonly DialogOverlay dialogOverlay = new DialogOverlay();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private CommentsContainer commentsContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.AddRange(new Drawable[]
            {
                content = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both
                },
                dialogOverlay
            });
        }

        [SetUpSteps]
        public void SetUp()
        {
            Schedule(() =>
            {
                API.Login("test", "test");
                Child = commentsContainer = new CommentsContainer();
            });
        }

        [Test]
        public void TestNonOwnCommentCantBeDeleted()
        {
            addTestComments();

            AddUntilStep("First comment has button", () =>
            {
                var comments = this.ChildrenOfType<DrawableComment>();
                var ourComment = comments.SingleOrDefault(x => x.Comment.Id == 1);
                return ourComment != null && ourComment.ChildrenOfType<OsuSpriteText>().Any(x => x.Text == "Delete");
            });

            AddAssert("Second doesn't", () =>
            {
                var comments = this.ChildrenOfType<DrawableComment>();
                var ourComment = comments.Single(x => x.Comment.Id == 2);
                return ourComment.ChildrenOfType<OsuSpriteText>().All(x => x.Text != "Delete");
            });
        }

        private readonly ManualResetEventSlim deletionPerformed = new ManualResetEventSlim();

        [Test]
        public void TestDeletion()
        {
            DrawableComment? ourComment = null;

            addTestComments();
            AddUntilStep("Comment exists", () =>
            {
                var comments = this.ChildrenOfType<DrawableComment>();
                ourComment = comments.SingleOrDefault(x => x.Comment.Id == 1);
                return ourComment != null;
            });
            AddStep("It has delete button", () =>
            {
                var btn = ourComment.ChildrenOfType<OsuSpriteText>().Single(x => x.Text == "Delete");
                InputManager.MoveMouseTo(btn);
            });
            AddStep("Click delete button", () =>
            {
                InputManager.Click(MouseButton.Left);
            });
            AddStep("Setup request handling", () =>
            {
                deletionPerformed.Reset();

                dummyAPI.HandleRequest = request =>
                {
                    if (!(request is CommentDeleteRequest req))
                        return false;

                    if (req.CommentId != 1)
                        return false;

                    CommentBundle cb = new CommentBundle
                    {
                        Comments = new List<Comment>
                        {
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

                    Task.Run(() =>
                    {
                        deletionPerformed.Wait(10000);
                        req.TriggerSuccess(cb);
                    });

                    return true;
                };
            });
            AddStep("Confirm dialog", () => InputManager.Key(Key.Number1));

            AddAssert("Loading spinner shown", () => commentsContainer.ChildrenOfType<LoadingSpinner>().Any(d => d.IsPresent));

            AddStep("Complete request", () => deletionPerformed.Set());

            AddUntilStep("Comment is deleted locally", () => this.ChildrenOfType<DrawableComment>().Single(x => x.Comment.Id == 1).WasDeleted);
        }

        [Test]
        public void TestDeletionFail()
        {
            DrawableComment? ourComment = null;
            bool delete = false;

            addTestComments();
            AddUntilStep("Comment exists", () =>
            {
                var comments = this.ChildrenOfType<DrawableComment>();
                ourComment = comments.SingleOrDefault(x => x.Comment.Id == 1);
                return ourComment != null;
            });
            AddStep("It has delete button", () =>
            {
                var btn = ourComment.ChildrenOfType<OsuSpriteText>().Single(x => x.Text == "Delete");
                InputManager.MoveMouseTo(btn);
            });
            AddStep("Click delete button", () =>
            {
                InputManager.Click(MouseButton.Left);
            });
            AddStep("Setup request handling", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (request is not CommentDeleteRequest req)
                        return false;

                    req.TriggerFailure(new Exception());
                    delete = true;
                    return false;
                };
            });
            AddStep("Confirm dialog", () => InputManager.Key(Key.Number1));
            AddUntilStep("Deletion requested", () => delete);
            AddUntilStep("Comment is available", () =>
            {
                return !this.ChildrenOfType<DrawableComment>().Single(x => x.Comment.Id == 1).WasDeleted;
            });
            AddAssert("Loading spinner hidden", () =>
            {
                return ourComment.ChildrenOfType<LoadingSpinner>().All(d => !d.IsPresent);
            });
            AddAssert("Actions available", () =>
            {
                return ourComment.ChildrenOfType<LinkFlowContainer>().Single(x => x.Name == @"Actions buttons").IsPresent;
            });
        }

        private void addTestComments()
        {
            AddStep("set up response", () =>
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
            });

            AddStep("show comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 123));
        }

        private void setUpCommentsResponse(CommentBundle commentBundle)
        {
            dummyAPI.HandleRequest = request =>
            {
                if (!(request is GetCommentsRequest getCommentsRequest))
                    return false;

                getCommentsRequest.TriggerSuccess(commentBundle);
                return true;
            };
        }
    }
}
