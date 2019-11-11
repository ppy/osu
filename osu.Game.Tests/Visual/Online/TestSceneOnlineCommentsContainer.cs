// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;
using osu.Game.Online.API.Requests;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneOnlineCommentsContainer : OsuTestScene
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

        public TestSceneOnlineCommentsContainer()
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
        }
    }
}
