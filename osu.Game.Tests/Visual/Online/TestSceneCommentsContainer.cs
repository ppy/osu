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
            typeof(ShowRepliesButton),
            typeof(DeletedCommentsPlaceholder),
            typeof(VotePill),
            typeof(GetCommentRepliesButton)
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
            AddStep("lazer build comments", () => commentsContainer.ShowComments(CommentableType.Build, 4772));
            AddStep("local comments", () => commentsContainer.ShowComments(commentBundle));
        }

        private readonly CommentBundle commentBundle = new CommentBundle
        {
            Comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Message = "Simple test comment",
                    LegacyName = "TestUser1",
                    CreatedAt = DateTimeOffset.Now,
                    DeletedAt = null,
                    EditedAt = null,
                }
            },
            TopLevelCount = 1,
        };
    }
}
