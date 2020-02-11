// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneCommentEditor : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CommentEditor),
            typeof(CancellableCommentEditor),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly OsuSpriteText text;
        private readonly TestCommentEditor commentEditor;
        private readonly TestCancellableCommentEditor cancellableCommentEditor;

        public TestSceneCommentEditor()
        {
            Add(new Container
            {
                AutoSizeAxes = Axes.Both,
                Child = text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont()
                }
            });

            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 800,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    commentEditor = new TestCommentEditor
                    {
                        OnCommit = onCommit
                    },
                    cancellableCommentEditor = new TestCancellableCommentEditor
                    {
                        OnCommit = onCommit
                    }
                }
            });
        }

        private void onCommit(string value)
        {
            text.Text = $@"Invoked text: {value}";

            Scheduler.AddDelayed(() =>
            {
                commentEditor.IsLoading = false;
                cancellableCommentEditor.IsLoading = false;
            }, 500);
        }

        private class TestCommentEditor : CommentEditor
        {
            protected override string FooterText => @"Footer text. And it is pretty long. Cool.";

            protected override string CommitButtonText => @"Commit";

            protected override string TextboxPlaceholderText => @"This textbox is empty";
        }

        private class TestCancellableCommentEditor : CancellableCommentEditor
        {
            protected override string FooterText => @"Wow, another one. Sicc";

            protected override string CommitButtonText => @"Save";

            protected override string TextboxPlaceholderText => @"Miltiline textboxes soon";
        }
    }
}
