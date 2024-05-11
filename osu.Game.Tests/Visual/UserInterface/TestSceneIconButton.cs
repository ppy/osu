// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneIconButton : OsuTestScene
    {
        public TestSceneIconButton()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 10),
                Children = new[]
                {
                    new NamedIconButton("No change", new IconButton()),
                    new NamedIconButton("Background colours", new ColouredIconButton()),
                    new NamedIconButton("Full-width", new IconButton { Size = new Vector2(200, 30) }),
                    new NamedIconButton("Icon colours", new IconButton
                    {
                        IconColour = Color4.Green,
                        IconHoverColour = Color4.Red
                    })
                }
            };
        }

        private partial class ColouredIconButton : IconButton
        {
            public ColouredIconButton()
            {
                FlashColour = Color4.DarkGreen;
                HoverColour = Color4.Green;
            }
        }

        private partial class NamedIconButton : Container
        {
            public NamedIconButton(string name, IconButton button)
            {
                AutoSizeAxes = Axes.Y;
                Width = 200;

                Container iconContainer;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = name
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.1f,
                                    },
                                    iconContainer = new Container
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Child = button
                                    }
                                }
                            }
                        }
                    }
                };

                iconContainer.AutoSizeAxes = Axes.Both;

                button.Anchor = Anchor.Centre;
                button.Origin = Anchor.Centre;
                button.Icon = OsuIcon.RulesetOsu;
            }
        }
    }
}
