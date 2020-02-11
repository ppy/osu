// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneCommentEditor : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CommentEditor),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneCommentEditor()
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 800,
                Child = new TestCommentEditor()
            });
        }

        private class TestCommentEditor : CommentEditor
        {
            protected override string FooterText => @"Footer text. And it is pretty long. Cool.";

            protected override string CommitButtonText => @"Commit";

            protected override string TextboxPlaceholderText => @"This textbox is empty";
        }
    }
}
