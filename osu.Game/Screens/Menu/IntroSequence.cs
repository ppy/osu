// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
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
        private const float logo_size = 460; //todo: this should probably be 480

        private OsuSpriteText welcomeText;

        private Container<Box> lines;

        private Box lineTopLeft;
        private Box lineBottomLeft;
        private Box lineTopRight;
        private Box lineBottomRight;

        private Ring smallRing;
        private Ring mediumRing;
        private Ring bigRing;

        private Box backgroundFill;
        private Box foregroundFill;

        private CircularContainer pinkCircle;
        private CircularContainer blueCircle;
        private CircularContainer yellowCircle;
        private CircularContainer purpleCircle;

        public IntroSequence()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const int line_offset = 80;
            const int circle_offset = 250;

            Children = new Drawable[]
            {
                lines = new Container<Box>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        lineTopLeft = new Box
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
                            Position = new Vector2(-line_offset, -line_offset),
                            Rotation = 45,
                            Colour = Color4.White.Opacity(180),
                        },
                        lineTopRight = new Box
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.Centre,
                            Position = new Vector2(line_offset, -line_offset),
                            Rotation = -45,
                            Colour = Color4.White.Opacity(80),
                        },
                        lineBottomLeft = new Box
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.Centre,
                            Position = new Vector2(-line_offset, line_offset),
                            Rotation = -45,
                            Colour = Color4.White.Opacity(230),
                        },
                        lineBottomRight = new Box
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.Centre,
                            Position = new Vector2(line_offset, line_offset),
                            Rotation = 45,
                            Colour = Color4.White.Opacity(130),
                        },
                    }
                },
                bigRing = new Ring(OsuColour.FromHex(@"B6C5E9"), 0.85f),
                mediumRing = new Ring(Color4.White.Opacity(130), 0.7f),
                smallRing = new Ring(Color4.White, 0.6f),
                welcomeText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "welcome",
                    Padding = new MarginPadding { Bottom = 10 },
                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                    Alpha = 0,
                    Spacing = new Vector2(5),
                },
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
                            Height = 0,
                            Colour = OsuColour.FromHex(@"C6D8FF").Opacity(160),
                        },
                        foregroundFill = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = Vector2.Zero,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0,
                            Colour = Color4.White,
                        },
                    }
                },
                purpleCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Position = new Vector2(0, circle_offset),
                    Colour = OsuColour.FromHex(@"AA92FF"),
                },
                blueCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    Position = new Vector2(-circle_offset, 0),
                    Colour = OsuColour.FromHex(@"8FE5FE"),
                },
                yellowCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Position = new Vector2(0, -circle_offset),
                    Colour = OsuColour.FromHex(@"FFD64C"),
                },
                pinkCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    Position = new Vector2(circle_offset, 0),
                    Colour = OsuColour.FromHex(@"e967a1"),
                },
            };

            foreach (var line in lines)
            {
                line.Size = new Vector2(105, 1.5f);
                line.Alpha = 0;
            }

            Scale = new Vector2(0.5f);
        }

        public void Start(double length)
        {
            if (Children.Any())
            {
                // restart if we were already run previously.
                FinishTransforms(true);
                load();
            }

            smallRing.ResizeTo(logo_size * 0.086f, 400, Easing.InOutQuint);

            mediumRing.ResizeTo(130, 340, Easing.OutQuad);
            mediumRing.Foreground.ResizeTo(1, 880, Easing.Out);

            double remainingTime() => length - TransformDelay;

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

                using (BeginDelayedSequence(length * 0.56, true))
                {
                    bigRing.ResizeTo(logo_size, 500, Easing.InOutQuint);
                    bigRing.Foreground.Delay(250).ResizeTo(1, 850, Easing.OutQuint);

                    using (BeginDelayedSequence(250, true))
                    {
                        backgroundFill.ResizeHeightTo(1, remainingTime(), Easing.InOutQuart);
                        backgroundFill.RotateTo(-90, remainingTime(), Easing.InOutQuart);

                        using (BeginDelayedSequence(50, true))
                        {
                            foregroundFill.ResizeWidthTo(1, remainingTime(), Easing.InOutQuart);
                            foregroundFill.RotateTo(-90, remainingTime(), Easing.InOutQuart);
                        }

                        this.ScaleTo(1, remainingTime(), Easing.InOutCubic);

                        const float circle_size = logo_size * 0.9f;

                        const int rotation_delay = 110;
                        const int appear_delay = 80;

                        purpleCircle.MoveToY(circle_size / 2, remainingTime(), Easing.InOutQuart);
                        purpleCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.InOutQuart);
                        purpleCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuart);

                        using (BeginDelayedSequence(appear_delay, true))
                        {
                            yellowCircle.MoveToY(-circle_size / 2, remainingTime(), Easing.InOutQuart);
                            yellowCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.InOutQuart);
                            yellowCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuart);

                            using (BeginDelayedSequence(appear_delay, true))
                            {
                                blueCircle.MoveToX(-circle_size / 2, remainingTime(), Easing.InOutQuart);
                                blueCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.InOutQuart);
                                blueCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuart);

                                using (BeginDelayedSequence(appear_delay, true))
                                {
                                    pinkCircle.MoveToX(circle_size / 2, remainingTime(), Easing.InOutQuart);
                                    pinkCircle.Delay(rotation_delay).RotateTo(-180, remainingTime() - rotation_delay, Easing.InOutQuart);
                                    pinkCircle.ResizeTo(circle_size, remainingTime(), Easing.InOutQuart);
                                }
                            }
                        }
                    }
                }
            }
        }

        private class Ring : Container<Circle>
        {
            public readonly Circle Foreground;

            public Ring(Color4 ringColour, float foregroundSize)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Children = new[]
                {
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.98f),
                        Colour = ringColour,
                    },
                    Foreground = new Circle
                    {
                        Size = new Vector2(foregroundSize),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    }
                };
            }
        }
    }
}
