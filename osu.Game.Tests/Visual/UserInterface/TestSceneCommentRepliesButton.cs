// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Comments.Buttons;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneCommentRepliesButton : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneCommentRepliesButton()
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new TestButton
                    {
                        Action = () => { }
                    },
                    new LoadRepliesButton
                    {
                        Action = () => { }
                    },
                    new ShowRepliesButton(1)
                    {
                        Action = () => { }
                    },
                    new ShowRepliesButton(2)
                    {
                        Action = () => { }
                    }
                }
            };
        }

        private class TestButton : CommentRepliesButton
        {
            protected override string GetText() => "sample text";
        }
    }
}
