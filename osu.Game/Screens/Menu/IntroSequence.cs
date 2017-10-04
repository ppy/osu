// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Menu
{
    public class IntroSequence : Container
    {
        private OsuSpriteText welcomeText;

        private readonly Container barTopLeft;
        private readonly Container barBottomLeft;
        private readonly Container barTopRight;
        private readonly Container barBottomRight;

        private readonly Ring smallRing;
        private readonly Ring mediumRing;
        private readonly Ring bigRing;

        private readonly Container backgroundFill;
        private readonly Container foregroundFill;

        private readonly CircularContainer pinkCircle;
        private readonly CircularContainer blueCircle;
        private readonly CircularContainer yellowCircle;
        private readonly CircularContainer purpleCircle;

        public IntroSequence()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(480),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        mediumRing = new Ring(Color4.White.Opacity(80)),
                        barTopLeft = new Container
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(100, 1.5f),
                            Position = new Vector2(-120, -120),
                            Rotation = 45,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White.Opacity(180),
                            }
                        },
                        barTopRight = new Container
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(100, 1.5f),
                            Position = new Vector2(120, -120),
                            Rotation = -45,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White.Opacity(80),
                            }
                        },
                        barBottomLeft = new Container
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(100, 1.5f),
                            Position = new Vector2(-120, 120),
                            Rotation = -45,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White.Opacity(230),
                            }
                        },
                        barBottomRight = new Container
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(100, 1.5f),
                            Position = new Vector2(120, 120),
                            Rotation = 45,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White.Opacity(130),
                            }
                        },
                        smallRing = new Ring(Color4.White),
                        bigRing = new Ring(OsuColour.FromHex(@"B6C5E9")),
                        backgroundFill = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex(@"C6D8FF").Opacity(160),
                            }
                        },
                        welcomeText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "welcome",
                            Font = @"Exo2.0-Light",
                            TextSize = 42,
                        },
                        foregroundFill = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                            }
                        },
                    }
                },
                purpleCircle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Purple,
                    }
                },
                yellowCircle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Yellow,
                    }
                },
                blueCircle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Blue,
                    }
                },
                pinkCircle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Pink,
                    }
                }
            };

            setDefaults();
        }

        public void Start()
        {
            welcomeText.FadeIn(1000);
            welcomeText.TransformSpacingTo(new Vector2(20, 0), 3000, Easing.OutQuint);

            smallRing.Background.ResizeTo(50, 1000, Easing.OutQuint);
            smallRing.Foreground.Delay(100).ResizeTo(52, 1000, Easing.OutQuint);

            mediumRing.Background.ResizeTo(100, 1000, Easing.OutQuint);
            mediumRing.Foreground.Delay(100).ResizeTo(102, 1000, Easing.OutQuint);

            bigRing.Background.Delay(1500).ResizeTo(400, 1000, Easing.OutQuint);
            bigRing.Foreground.Delay(1600).ResizeTo(402, 1000, Easing.OutQuint);

            backgroundFill.Delay(2500).ResizeHeightTo(250, 500, Easing.OutQuint);
            backgroundFill.Delay(2500).RotateTo(-45, 500, Easing.OutQuint);

            foregroundFill.Delay(2500).ResizeWidthTo(500, 1000, Easing.OutQuint);
            foregroundFill.Delay(2500).RotateTo(-90, 1000, Easing.OutQuint);

            yellowCircle.Delay(3500).MoveToY(-220, 1000);
            yellowCircle.Delay(3500).RotateTo(-180, 1000);
            yellowCircle.Delay(3500).ResizeTo(438, 1000);

            purpleCircle.Delay(3500).MoveToY(220, 1000);
            purpleCircle.Delay(3500).RotateTo(-180, 1000);
            purpleCircle.Delay(3500).ResizeTo(438, 1000);

            blueCircle.Delay(3500).MoveToX(-220, 1000);
            blueCircle.Delay(3500).RotateTo(-180, 1000);
            blueCircle.Delay(3500).ResizeTo(438, 1000);

            pinkCircle.Delay(3500).MoveToX(220, 1000);
            pinkCircle.Delay(3500).RotateTo(-180, 1000);
            pinkCircle.Delay(3500).ResizeTo(440, 1000);
        }

        private void setDefaults()
        {
            welcomeText.Alpha = 0;
            welcomeText.Spacing = Vector2.Zero;

            smallRing.Background.Size = smallRing.Foreground.Size = Vector2.Zero;
            mediumRing.Background.Size = mediumRing.Foreground.Size = Vector2.Zero;
            bigRing.Background.Size = bigRing.Foreground.Size = Vector2.Zero;

            backgroundFill.Rotation = 0;
            backgroundFill.Size = new Vector2(500, 0);

            foregroundFill.Rotation = 0;
            foregroundFill.Size = new Vector2(0, 500);

            yellowCircle.Position = new Vector2(0, -300);
            yellowCircle.Size = Vector2.Zero;
            yellowCircle.Rotation = 0;

            purpleCircle.Position = new Vector2(0, 300);
            purpleCircle.Size = Vector2.Zero;
            purpleCircle.Rotation = 0;

            blueCircle.Position = new Vector2(-300, 0);
            blueCircle.Size = Vector2.Zero;
            blueCircle.Rotation = 0;

            pinkCircle.Position = new Vector2(300, 0);
            pinkCircle.Size = Vector2.Zero;
            pinkCircle.Rotation = 0;
        }

        public void Restart()
        {
            FinishTransforms(true);
            setDefaults();
            Start();
        }

        private class Ring : Container<CircularContainer>
        {
            public CircularContainer Background;
            public CircularContainer Foreground;

            public Ring(Color4 ringColour)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;
                Children = new[]
                {
                    Background = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ringColour,
                        }
                    },
                    Foreground = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                        }
                    }
                };
            }
        }
    }
}
