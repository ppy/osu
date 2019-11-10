// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneCommentsHeader : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CommentsHeader),
            typeof(HeaderButton),
            typeof(SortTabControl),
        };

        private readonly Bindable<CommentsSortCriteria> sort = new Bindable<CommentsSortCriteria>();
        private readonly BindableBool showDeleted = new BindableBool();

        public TestSceneCommentsHeader()
        {
            Add(new CommentsHeader
            {
                Sort = { BindTarget = sort },
                ShowDeleted = { BindTarget = showDeleted }
            });

            AddStep("Trigger ShowDeleted", () => showDeleted.Value = !showDeleted.Value);
            AddStep("Select old", () => sort.Value = CommentsSortCriteria.Old);
            AddStep("Select new", () => sort.Value = CommentsSortCriteria.New);
            AddStep("Select top", () => sort.Value = CommentsSortCriteria.Top);
        }
    }
}
