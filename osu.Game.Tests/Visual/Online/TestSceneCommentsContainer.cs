// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;
using osu.Game.Overlays;
using osu.Framework.Allocation;

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

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneCommentsContainer()
        {
            BasicScrollContainer scroll;
            CommentsContainer comments;

            Add(scroll = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = comments = new CommentsContainer()
            });

            AddStep("Big Black comments", () => comments.ShowComments(CommentableType.Beatmapset, 41823));
            AddStep("Airman comments", () => comments.ShowComments(CommentableType.Beatmapset, 24313));
            AddStep("Lazer build comments", () => comments.ShowComments(CommentableType.Build, 4772));
            AddStep("News comments", () => comments.ShowComments(CommentableType.NewsPost, 715));
            AddStep("Idle state", () =>
            {
                scroll.Clear();
                scroll.Add(comments = new CommentsContainer());
            });
        }
    }
}