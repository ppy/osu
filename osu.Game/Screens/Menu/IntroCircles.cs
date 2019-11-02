// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class IntroCircles : IntroScreen
    {
        protected override string BeatmapHash => "3c8b1fcc9434dbb29e2fb613d3b9eada9d7bb6c125ceb32396c3b53437280c83";

        protected override string BeatmapFile => "circles.osz";

        private const double load_menu_delay = 2400;
        private const double start_track_delay = 600;

        private SampleChannel welcome;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            if (MenuVoice.Value)
                welcome = audio.Samples.Get(@"welcome");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                PrepareMenuLoad();

                LoadComponentAsync(new CirclesIntroSequence(logo), c =>
                {
                    AddInternal(c);
                    welcome?.Play();

                    Scheduler.AddDelayed(() =>
                    {
                        StartTrack();

                        Scheduler.AddDelayed(LoadMenu, load_menu_delay);
                    }, start_track_delay);
                });
            }
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(300);
            base.OnSuspending(next);
        }

        private class CirclesIntroSequence : CompositeDrawable
        {
            private const float logo_size = 480;

            private readonly OsuLogo logo;

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

            private readonly Circle pinkCircle;
            private readonly Circle blueCircle;
            private readonly Circle yellowCircle;
            private readonly Circle purpleCircle;

            public CirclesIntroSequence(OsuLogo logo)
            {
                const float line_offset = 80;
                const float circle_offset = 250;

                this.logo = logo;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                Scale = new Vector2(0.5f);

                InternalChildren = new Drawable[]
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
            }

            protected override void LoadComplete()
            {
                const double intro_length = 3150;
                const double fade_duration = 200;

                base.LoadComplete();

                smallRing.ResizeTo(logo_size * 0.086f, 400, Easing.InOutQuint);

                mediumRing.ResizeTo(130, 340, Easing.OutQuad);
                mediumRing.Foreground.ResizeTo(1, 880, Easing.Out);

                double remainingTime() => intro_length - TransformDelay;

                using (BeginDelayedSequence(250, true))
                {
                    welcomeText.FadeIn(700);
                    welcomeText.TransformSpacingTo(new Vector2(20, 0), remainingTime(), Easing.Out);

                    const double line_duration = 700;
                    const float line_resize = 150;

                    foreach (var line in lines)
                    {
                        line.FadeIn(40).ResizeWidthTo(0, line_duration - line_resize, Easing.OutQuint);
                    }

                    const float line_end_offset = 120;

                    smallRing.Foreground.ResizeTo(1, line_duration, Easing.OutQuint);

                    lineTopLeft.MoveTo(new Vector2(-line_end_offset, -line_end_offset), line_duration, Easing.OutQuint);
                    lineTopRight.MoveTo(new Vector2(line_end_offset, -line_end_offset), line_duration, Easing.OutQuint);
                    lineBottomLeft.MoveTo(new Vector2(-line_end_offset, line_end_offset), line_duration, Easing.OutQuint);
                    lineBottomRight.MoveTo(new Vector2(line_end_offset, line_end_offset), line_duration, Easing.OutQuint);

                    using (BeginDelayedSequence(intro_length * 0.56, true))
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

                            const double rotation_delay = 110;
                            const double appear_delay = 80;

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

                logo.FadeOut().Delay(intro_length).FadeIn(fade_duration);
                this.Delay(intro_length + fade_duration).FadeOut();
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
}
