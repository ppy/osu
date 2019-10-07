// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Online.API.Requests;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneCommentsContainer : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CommentsContainer),
        };

        public TestSceneCommentsContainer()
        {
            AddStep("Big Black comments", () =>
            {
                Clear();
                Add(new CommentsContainer(CommentableType.Beatmapset, 41823));
            });
        }
    }
}
