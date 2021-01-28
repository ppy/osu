﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments.Buttons;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneCommentRepliesButton : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly TestButton button;

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
                    button = new TestButton(),
                    new LoadRepliesButton
                    {
                        Action = () => { }
                    },
                    new ShowRepliesButton(1),
                    new ShowRepliesButton(2)
                }
            };
        }

        [Test]
        public void TestArrowDirection()
        {
            AddStep("Set upwards", () => button.SetIconDirection(true));
            AddAssert("Icon facing upwards", () => button.Icon.Scale.Y == -1);
            AddStep("Set downwards", () => button.SetIconDirection(false));
            AddAssert("Icon facing downwards", () => button.Icon.Scale.Y == 1);
        }

        private class TestButton : CommentRepliesButton
        {
            public SpriteIcon Icon => this.ChildrenOfType<SpriteIcon>().First();

            public TestButton()
            {
                Text = "sample text";
            }

            public new void SetIconDirection(bool upwards) => base.SetIconDirection(upwards);
        }
    }
}
