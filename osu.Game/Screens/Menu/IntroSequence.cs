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
        private readonly OsuSpriteText welcomeText;

        private readonly OsuLogo logo;

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
                    Size = new Vector2(460),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        mediumRing = new Ring(Color4.White.Opacity(80)),
                        barTopLeft = new Container
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
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
                            RelativeSizeAxes = Axes.Both,
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
                            RelativeSizeAxes = Axes.Both,
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
                        Colour = OsuColour.FromHex(@"AA92FF"),
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
                        Colour = OsuColour.FromHex(@"FFD64C"),
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
                        Colour = OsuColour.FromHex(@"8FE5FE"),
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
                        Colour = OsuColour.FromHex(@"e967a1"),
                    }
                },
                logo = new OsuLogo
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Ripple = false,
                    Interactive = false,
                    Blending = BlendingMode.Additive,
                },
            };

            setDefaults();
        }

        public void Start()
        {
            welcomeText.Delay(350).ScaleTo(1, 250, Easing.Out);
            welcomeText.Delay(350).FadeIn(1000, Easing.Out);
            welcomeText.Delay(350).TransformSpacingTo(new Vector2(20, 0), 1450, Easing.Out);

            mediumRing.ResizeTo(120, 500, Easing.InExpo);
            mediumRing.Foreground.Delay(620).ResizeTo(1, 1000, Easing.OutQuint);

            smallRing.Delay(200).ResizeTo(45, 500, Easing.InExpo);
            smallRing.Foreground.Delay(700).ResizeTo(1, 2000, Easing.OutQuint);

            barTopLeft.Delay(700).FadeIn();
            barTopLeft.Delay(700).MoveTo(new Vector2(-120, -120), 900, Easing.OutQuint);
            barTopLeft.Delay(800).ResizeWidthTo(0, 900, Easing.OutExpo);

            barTopRight.Delay(700).FadeIn();
            barTopRight.Delay(700).MoveTo(new Vector2(120, -120), 900, Easing.OutQuint);
            barTopRight.Delay(800).ResizeWidthTo(0, 900, Easing.OutExpo);

            barBottomLeft.Delay(700).FadeIn();
            barBottomLeft.Delay(700).MoveTo(new Vector2(-120, 120), 900, Easing.OutQuint);
            barBottomLeft.Delay(800).ResizeWidthTo(0, 900, Easing.OutExpo);

            barBottomRight.Delay(700).FadeIn();
            barBottomRight.Delay(700).MoveTo(new Vector2(120, 120), 900, Easing.OutQuint);
            barBottomRight.Delay(800).ResizeWidthTo(0, 900, Easing.OutExpo);

            bigRing.Delay(1950).ResizeTo(400, 550, Easing.InOutQuint);
            bigRing.Foreground.Delay(1950).ResizeTo(0.8f, 450, Easing.InExpo).Then().ResizeTo(1, 500, Easing.OutExpo);

            backgroundFill.Delay(2317).ResizeHeightTo(1, 650, Easing.InOutQuint);
            backgroundFill.Delay(2317).RotateTo(-90, 650, Easing.InOutQuint);

            foregroundFill.Delay(2350).ResizeWidthTo(1, 650, Easing.InOutQuint);
            foregroundFill.Delay(2350).RotateTo(-90, 650, Easing.InOutQuint);

            yellowCircle.Delay(2383).MoveToY(-207, 617, Easing.InOutQuad);
            yellowCircle.Delay(2383).RotateTo(-180, 617, Easing.InOutQuad);
            yellowCircle.Delay(2383).ResizeTo(414, 617, Easing.InOutExpo);

            purpleCircle.Delay(2317).MoveToY(207, 683, Easing.InOutQuad);
            purpleCircle.Delay(2317).RotateTo(-180, 683, Easing.InOutQuad);
            purpleCircle.Delay(2317).ResizeTo(414, 683, Easing.InOutExpo);

            blueCircle.Delay(2449).MoveToX(-207, 551, Easing.InOutQuad);
            blueCircle.Delay(2449).RotateTo(-180, 551, Easing.InOutQuad);
            blueCircle.Delay(2449).ResizeTo(414, 551, Easing.InOutExpo);

            pinkCircle.Delay(2515).MoveToX(208, 485, Easing.InOutQuad);
            pinkCircle.Delay(2515).RotateTo(-180, 485, Easing.InOutQuad);
            pinkCircle.Delay(2515).ResizeTo(416, 485, Easing.InOutExpo);

            logo.Delay(3200).FadeIn(300);

            backgroundFill.Delay(3200).FadeOut();
            foregroundFill.Delay(3500).FadeOut();
        }

        private void setDefaults()
        {
            logo.Alpha = 0;

            welcomeText.Scale = new Vector2(0.9f);
            welcomeText.Spacing = Vector2.Zero;
            welcomeText.Alpha = 0;

            smallRing.Size = mediumRing.Size = bigRing.Size = Vector2.Zero;
            mediumRing.Foreground.Size = new Vector2(0.8f);
            smallRing.Foreground.Size = new Vector2(0.7f);
            bigRing.Foreground.Size = Vector2.Zero;

            barTopLeft.Size = barTopRight.Size = barBottomLeft.Size = barBottomRight.Size = new Vector2(115, 1.5f);
            barTopLeft.Alpha = barTopRight.Alpha = barBottomLeft.Alpha = barBottomRight.Alpha = 0;
            barTopLeft.Position = new Vector2(-90, -90);
            barTopRight.Position = new Vector2(90, -90);
            barBottomLeft.Position = new Vector2(-90, 90);
            barBottomRight.Position = new Vector2(90, 90);

            backgroundFill.Rotation = foregroundFill.Rotation = 0;
            backgroundFill.Alpha = foregroundFill.Alpha = 1;
            backgroundFill.Height = foregroundFill.Width = 0;

            yellowCircle.Size = purpleCircle.Size = blueCircle.Size = pinkCircle.Size = Vector2.Zero;
            yellowCircle.Rotation = purpleCircle.Rotation = blueCircle.Rotation = pinkCircle.Rotation = 0;
            yellowCircle.Position = new Vector2(0, -300);
            purpleCircle.Position = new Vector2(0, 300);
            blueCircle.Position = new Vector2(-300, 0);
            pinkCircle.Position = new Vector2(300, 0);
        }

        public void Restart()
        {
            FinishTransforms(true);
            setDefaults();
            Start();
        }

        private class Ring : CircularContainer
        {
            public readonly CircularContainer Foreground;

            public Ring(Color4 ringColour)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Masking = true;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ringColour,
                    },
                    Foreground = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
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
