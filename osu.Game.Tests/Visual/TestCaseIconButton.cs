// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual
{
    public class TestCaseIconButton : OsuTestCase
    {
        public override string Description => "Various display modes of icon buttons";

        public TestCaseIconButton()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 10),
                Children = new[]
                {
                    new NamedIconButton("No change", new IconButton()),
                    new NamedIconButton("Background colours", new IconButton
                    {
                        FlashColour = Color4.DarkGreen,
                        HoverColour = Color4.Green,
                    }),
                    new NamedIconButton("Full-width", new IconButton { ButtonSize = new Vector2(200, 30) }),
                    new NamedIconButton("Unchanging size", new IconButton(), false),
                    new NamedIconButton("Icon colours", new IconButton
                    {
                        IconColour = Color4.Green,
                        IconHoverColour = Color4.Red
                    })
                }
            };
        }

        private class NamedIconButton : Container
        {
            public NamedIconButton(string name, IconButton button, bool allowSizeChange = true)
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

                if (allowSizeChange)
                    iconContainer.AutoSizeAxes = Axes.Both;
                else
                {
                    iconContainer.RelativeSizeAxes = Axes.X;
                    iconContainer.Height = 30;
                }

                button.Anchor = Anchor.Centre;
                button.Origin = Anchor.Centre;
                button.Icon = FontAwesome.fa_osu_osu_o;
            }
        }
    }
}
