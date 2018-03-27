// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multiplayer
{
    public class Lobby : OsuScreen
    {
        public Lobby()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"3e3a44"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = OsuColour.FromHex(@"3c3842"),
                            ColourDark = OsuColour.FromHex(@"393540"),
                            TriangleScale = 5,
                        },
                    },
                },
                new Header(),
            };
        }

        private class Header : Container
        {
            private readonly OsuSpriteText screenType;
            private readonly Box tabStrip;
            private readonly BreadcrumbControl<string> breadcrumbs;

            public Header()
            {
                RelativeSizeAxes = Axes.X;
                Height = 121;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"2f2043"),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 80 },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.BottomLeft,
                                Position = new Vector2(-35f, 5f),
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(10f, 0f),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Size = new Vector2(25),
                                        Icon = FontAwesome.fa_osu_multi,
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Children = new[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = "multiplayer ",
                                                TextSize = 25,
                                            },
                                            screenType = new OsuSpriteText
                                            {
                                                TextSize = 25,
                                                Font = @"Exo2.0-Light",
                                            },
                                        },
                                    },
                                },
                            },
                            tabStrip = new Box
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Width = 0,
                                Height = 1,
                            },
                            breadcrumbs = new BreadcrumbControl<string>
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.X,
                                OnLoadComplete = d => { breadcrumbs.AccentColour = Color4.White; },
                            },
                        },
                    },
                };

                screenType.Text = @"lobby";
                breadcrumbs.AddItem(@"Lounge");
                breadcrumbs.AddItem(@"One Awesome Room");
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                screenType.Colour = colours.Yellow;
                tabStrip.Colour = colours.Green;
            }
        }
    }
}
