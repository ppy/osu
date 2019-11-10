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
            typeof(ShowChildrenButton),
            typeof(DeletedChildrenPlaceholder),
            typeof(VotePill)
        };

        protected override bool UseOnlineAPI => true;

        public TestSceneCommentsContainer()
        {
            BasicScrollContainer scrollFlow;

            Add(scrollFlow = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep("Big Black comments", () =>
            {
                scrollFlow.Clear();
                scrollFlow.Add(new CommentsContainer(CommentableType.Beatmapset, 41823));
            });

            AddStep("Airman comments", () =>
            {
                scrollFlow.Clear();
                scrollFlow.Add(new CommentsContainer(CommentableType.Beatmapset, 24313));
            });

            AddStep("lazer build comments", () =>
            {
                scrollFlow.Clear();
                scrollFlow.Add(new CommentsContainer(CommentableType.Build, 4772));
            });
        }
    }
}
