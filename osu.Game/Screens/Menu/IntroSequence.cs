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
        //Size
        private const int logo_size = 460; //todo: this should probably be 480

        private readonly OsuSpriteText welcomeText;

        private readonly OsuLogo logo;

        private readonly Container barsContainer;

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
                mediumRing = new Ring(Color4.White.Opacity(130)),
                barsContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
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
                    }
                },
                smallRing = new Ring(Color4.White),
                bigRing = new Ring(OsuColour.FromHex(@"B6C5E9")),
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(logo_size),
                    Masking = true,
                    Children = new Drawable[]
                    {
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
            const int circle_size = 416;

            //Time
            const int full_animation_duration = 2950;

            const int medium_ring_resize_duration = 360;
            const int medium_ring_fade_duration = 420;

            const int small_ring_resize_duration = 250;
            const int small_ring_fade_duration = 650;

            const int text_fade_duration = 700;
            const int text_spacing_transform_duration = 1500;

            const int bar_animation_duration = 700;
            const int bar_resize_delay = 150;

            const int big_ring_animation_start_delay = 2000;
            const int big_ring_resize_duration = 500;
            const int big_ring_foreground_resize_delay = 250;
            const int big_ring_fade_duration = 450;

            const int background_animation_start_time = 2250;
            const int foreground_animation_start_time = 2300;

            const int colored_circle_rotation_delay = 110;
            const int colored_circles_appear_delay = 80;
            const int purple_circle_animation_start_time = 2250;

            const int logo_fade_duration = 300;

            //Position
            const int bar_end_offset = 120;

            mediumRing.ResizeTo(130, medium_ring_resize_duration, Easing.InExpo);

            using (BeginDelayedSequence(200, true))
            {
                welcomeText.FadeIn(text_fade_duration);
                welcomeText.TransformSpacingTo(new Vector2(20, 0), text_spacing_transform_duration, Easing.Out);

                smallRing.ResizeTo(40, small_ring_resize_duration, Easing.InExpo);
                smallRing.Foreground.Delay(small_ring_resize_duration).ResizeTo(1, small_ring_fade_duration, Easing.OutQuad);
            }

            using (BeginDelayedSequence(medium_ring_resize_duration, true))
            {
                mediumRing.Foreground.ResizeTo(1, medium_ring_fade_duration, Easing.OutQuad);

                foreach (var bar in barsContainer)
                {
                    bar.FadeIn();
                    bar.Delay(bar_resize_delay).ResizeWidthTo(0, bar_animation_duration - bar_resize_delay, Easing.OutQuint);
                }

                barTopLeft.MoveTo(new Vector2(-bar_end_offset, -bar_end_offset), bar_animation_duration, Easing.OutQuint);
                barTopRight.MoveTo(new Vector2(bar_end_offset, -bar_end_offset), bar_animation_duration, Easing.OutQuint);
                barBottomLeft.MoveTo(new Vector2(-bar_end_offset, bar_end_offset), bar_animation_duration, Easing.OutQuint);
                barBottomRight.MoveTo(new Vector2(bar_end_offset, bar_end_offset), bar_animation_duration, Easing.OutQuint);
            }

            using (bigRing.BeginDelayedSequence(big_ring_animation_start_delay, true))
            {
                bigRing.ResizeTo(400, big_ring_resize_duration, Easing.InOutQuint);
                bigRing.Foreground.Delay(big_ring_foreground_resize_delay).ResizeTo(1, big_ring_fade_duration, Easing.OutExpo);
            }

            int duration = full_animation_duration - background_animation_start_time;
            using (backgroundFill.BeginDelayedSequence(background_animation_start_time))
            {
                backgroundFill.ResizeHeightTo(1, duration, Easing.InOutQuart);
                backgroundFill.RotateTo(-90, duration, Easing.InOutQuart);
            }

            duration = full_animation_duration - foreground_animation_start_time;
            using (foregroundFill.BeginDelayedSequence(foreground_animation_start_time))
            {
                foregroundFill.ResizeWidthTo(1, duration, Easing.InOutQuart);
                foregroundFill.RotateTo(-90, duration, Easing.InOutQuart);
            }

            duration = full_animation_duration - purple_circle_animation_start_time;
            using (BeginDelayedSequence(purple_circle_animation_start_time, true))
            {
                purpleCircle.MoveToY((circle_size - 2) / 2.0f, duration, Easing.InOutQuad);
                purpleCircle.Delay(colored_circle_rotation_delay).RotateTo(-180, duration - colored_circle_rotation_delay, Easing.OutQuad);
                purpleCircle.ResizeTo(circle_size - 2, duration, Easing.InOutQuad);

                duration -= colored_circles_appear_delay;
                using (BeginDelayedSequence(colored_circles_appear_delay, true))
                {
                    yellowCircle.MoveToY(-(circle_size - 2) / 2.0f, duration, Easing.InOutQuad);
                    yellowCircle.Delay(colored_circle_rotation_delay).RotateTo(-180, duration - colored_circle_rotation_delay, Easing.OutQuad);
                    yellowCircle.ResizeTo(circle_size - 2, duration, Easing.InOutQuad);

                    duration -= colored_circles_appear_delay;
                    using (BeginDelayedSequence(colored_circles_appear_delay, true))
                    {
                        blueCircle.MoveToX(-(circle_size - 2) / 2.0f, duration, Easing.InOutQuad);
                        blueCircle.Delay(colored_circle_rotation_delay).RotateTo(-180, duration - colored_circle_rotation_delay, Easing.OutQuad);
                        blueCircle.ResizeTo(circle_size - 2, duration, Easing.InOutQuad);

                        duration -= colored_circles_appear_delay;
                        using (BeginDelayedSequence(colored_circles_appear_delay, true))
                        {
                            pinkCircle.MoveToX(circle_size / 2.0f, duration, Easing.InOutQuad);
                            pinkCircle.Delay(colored_circle_rotation_delay).RotateTo(-180, duration - colored_circle_rotation_delay, Easing.OutQuad);
                            pinkCircle.ResizeTo(circle_size, duration, Easing.InOutQuad);
                        }
                    }
                }
            }

            logo.Delay(full_animation_duration).FadeIn(logo_fade_duration);

            backgroundFill.Delay(full_animation_duration + logo_fade_duration).FadeOut();
            foregroundFill.Delay(full_animation_duration + logo_fade_duration).FadeOut();
        }

        private void setDefaults()
        {
            logo.Alpha = 0;

            welcomeText.Spacing = new Vector2(5);
            welcomeText.Alpha = 0;

            smallRing.Size = mediumRing.Size = bigRing.Size = Vector2.Zero;

            mediumRing.Foreground.Size = Vector2.One - new Vector2(0.7f);
            smallRing.Foreground.Size = Vector2.One - new Vector2(0.4f);
            bigRing.Foreground.Size = Vector2.One - new Vector2(0.15f);

            barTopLeft.Size = barTopRight.Size = barBottomLeft.Size = barBottomRight.Size = new Vector2(105, 1.5f);
            barTopLeft.Alpha = barTopRight.Alpha = barBottomLeft.Alpha = barBottomRight.Alpha = 0;

            const int bar_offset = 80;
            barTopLeft.Position = new Vector2(-bar_offset, -bar_offset);
            barTopRight.Position = new Vector2(bar_offset, -bar_offset);
            barBottomLeft.Position = new Vector2(-bar_offset, bar_offset);
            barBottomRight.Position = new Vector2(bar_offset, bar_offset);

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

        public void Restart()
        {
            FinishTransforms(true);
            setDefaults();
            Start();
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
