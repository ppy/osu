// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
        //Size
        private const float logo_size = 460; //todo: this should probably be 480

        private readonly OsuSpriteText welcomeText;

        private readonly Container<Box> lines;

        private readonly Box lineTopLeft;
        private readonly Box lineBottomLeft;
        private readonly Box lineTopRight;
        private readonly Box lineBottomRight;

        private readonly Ring smallRing;
        private readonly Ring mediumRing;
        private readonly Ring bigRing;

        private readonly Box backgroundFill;
        private readonly Box foregroundFill;

        private readonly CircularContainer pinkCircle;
        private readonly CircularContainer blueCircle;
        private readonly CircularContainer yellowCircle;
        private readonly CircularContainer purpleCircle;

        public IntroSequence()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                lines = new Container<Box>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new []
                    {
                        lineTopLeft = new Box
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
                            Rotation = 45,
                            Colour = Color4.White.Opacity(180),
                        },
                        lineTopRight = new Box
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.Centre,
                            Rotation = -45,
                            Colour = Color4.White.Opacity(80),
                        },
                        lineBottomLeft = new Box
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
                            Rotation = -45,
                            Colour = Color4.White.Opacity(230),
                        },
                        lineBottomRight = new Box
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.Centre,
                            Rotation = 45,
                            Colour = Color4.White.Opacity(130),
                        },
                    }
                },
                bigRing = new Ring(OsuColour.FromHex(@"B6C5E9")),
                mediumRing = new Ring(Color4.White.Opacity(130)),
                smallRing = new Ring(Color4.White),
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(logo_size),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        backgroundFill = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"C6D8FF").Opacity(160),
                        },
                        welcomeText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "welcome",
                            Padding = new MarginPadding { Bottom = 10 },
                            Font = @"Exo2.0-Light",
                            TextSize = 42,
                        },
                        foregroundFill = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                    }
                },
                purpleCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Masking = true,
                    Colour = OsuColour.FromHex(@"AA92FF"),
                },
                yellowCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Masking = true,
                    Colour = OsuColour.FromHex(@"FFD64C"),
                },
                blueCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    Masking = true,
                    Colour = OsuColour.FromHex(@"8FE5FE"),
                },
                pinkCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    Masking = true,
                    Colour = OsuColour.FromHex(@"e967a1"),
                },
            };
        }

        private void setDefaults()
        {
            welcomeText.Spacing = new Vector2(5);
            welcomeText.Alpha = 0;

            smallRing.Size = mediumRing.Size = bigRing.Size = Vector2.Zero;

            bigRing.Foreground.Size = new Vector2(0.85f);
            mediumRing.Foreground.Size = new Vector2(0.7f);
            smallRing.Foreground.Size = new Vector2(0.6f);

            foreach (var line in lines)
            {
                line.Size = new Vector2(105, 1.5f);
                line.Alpha = 0;
            }

            const int line_offset = 80;
            lineTopLeft.Position = new Vector2(-line_offset, -line_offset);
            lineTopRight.Position = new Vector2(line_offset, -line_offset);
            lineBottomLeft.Position = new Vector2(-line_offset, line_offset);
            lineBottomRight.Position = new Vector2(line_offset, line_offset);

            backgroundFill.Rotation = foregroundFill.Rotation = 0;
            backgroundFill.Alpha = foregroundFill.Alpha = 1;
            backgroundFill.Height = foregroundFill.Width = 0;

            yellowCircle.Size = purpleCircle.Size = blueCircle.Size = pinkCircle.Size = Vector2.Zero;
            yellowCircle.Rotation = purpleCircle.Rotation = blueCircle.Rotation = pinkCircle.Rotation = 0;

            const int circle_offset = 250;
            yellowCircle.Position = new Vector2(0, -circle_offset);
            purpleCircle.Position = new Vector2(0, circle_offset);
            blueCircle.Position = new Vector2(-circle_offset, 0);
            pinkCircle.Position = new Vector2(circle_offset, 0);
        }

        public void Start(double length)
        {
            FinishTransforms(true);
            setDefaults();

            smallRing.ResizeTo(logo_size * 0.086f, 400, Easing.InOutQuint);

            mediumRing.ResizeTo(130, 340, Easing.OutQuad);
            mediumRing.Foreground.ResizeTo(1, 880, Easing.Out);

            Func<double> remainingTime = () => length - TransformDelay;

            using (BeginDelayedSequence(250, true))
            {
                welcomeText.FadeIn(700);
                welcomeText.TransformSpacingTo(new Vector2(20, 0), remainingTime(), Easing.Out);

                const int line_duration = 700;
                const int line_resize = 150;

                foreach (var line in lines)
                {
                    line.FadeIn(40).ResizeWidthTo(0, line_duration - line_resize, Easing.OutQuint);
                }

                const int line_end_offset = 120;

                smallRing.Foreground.ResizeTo(1, line_duration, Easing.OutQuint);

                lineTopLeft.MoveTo(new Vector2(-line_end_offset, -line_end_offset), line_duration, Easing.OutQuint);
                lineTopRight.MoveTo(new Vector2(line_end_offset, -line_end_offset), line_duration, Easing.OutQuint);
                lineBottomLeft.MoveTo(new Vector2(-line_end_offset, line_end_offset), line_duration, Easing.OutQuint);
                lineBottomRight.MoveTo(new Vector2(line_end_offset, line_end_offset), line_duration, Easing.OutQuint);

                using (BeginDelayedSequence(1640, true)) // 2000
                {
                    bigRing.ResizeTo(logo_size * 0.86f, 500, Easing.InOutQuint);
                    bigRing.Foreground.Delay(250).ResizeTo(1, 450, Easing.OutExpo);

                    using (BeginDelayedSequence(250, true)) // 2250
                    {
                        backgroundFill.ResizeHeightTo(1, remainingTime(), Easing.InOutQuart);
                        backgroundFill.RotateTo(-90, remainingTime(), Easing.InOutQuart);

                        using (BeginDelayedSequence(50, true))
                        {
                            foregroundFill.ResizeWidthTo(1, remainingTime(), Easing.InOutQuart);
                            foregroundFill.RotateTo(-90, remainingTime(), Easing.InOutQuart);
                        }

                        const float circle_size = logo_size * 0.9f;

                        const int rotation_delay = 110;
                        const int appear_delay = 80;

                        purpleCircle.MoveToY(circle_size / 2, remainingTime(), Easing.InOutQuad);
                        purpleCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.OutQuad);
                        purpleCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuad);

                        using (BeginDelayedSequence(appear_delay, true))
                        {
                            yellowCircle.MoveToY(-circle_size / 2, remainingTime(), Easing.InOutQuad);
                            yellowCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.OutQuad);
                            yellowCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuad);

                            using (BeginDelayedSequence(appear_delay, true))
                            {
                                blueCircle.MoveToX(-circle_size / 2, remainingTime(), Easing.InOutQuad);
                                blueCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.OutQuad);
                                blueCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuad);

                                using (BeginDelayedSequence(appear_delay, true))
                                {
                                    pinkCircle.MoveToX(circle_size / 2, remainingTime(), Easing.InOutQuad);
                                    pinkCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.OutQuad);
                                    pinkCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuad);
                                }
                            }
                        }
                    }
                }
            }
        }

        private class Ring : Container<CircularContainer>
        {
            public readonly CircularContainer Foreground;

            public Ring(Color4 ringColour)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Children = new[]
                {
                    new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Scale = new Vector2(0.98f),
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
