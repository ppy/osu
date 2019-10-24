// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;

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

            AddStep("Initial creation", () =>
            {
                scroll.Clear();
                scroll.Add(commentsContainer = new CommentsContainer());
            });
            AddStep("Big Black comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 41823));
            AddStep("Airman comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 24313));
            AddStep("lazer build comments", () => commentsContainer.ShowComments(CommentableType.Build, 4772));
            AddStep("beatmap with no comments", () => commentsContainer.ShowComments(CommentableType.Beatmapset, 1293));
        }
    }
}
