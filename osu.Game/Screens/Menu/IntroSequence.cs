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
        private const int logo_size = 460;

        private const int small_ring_size = 40;
        private const int medium_ring_size = 130;
        private const int big_ring_size = 400;

        private static readonly Vector2 medium_ring_thickness = new Vector2(0.3f);
        private static readonly Vector2 small_ring_thickness = new Vector2(0.6f);
        private static readonly Vector2 big_ring_thickness = new Vector2(0.85f);

        private static readonly Vector2 bar_size = new Vector2(105, 1.5f);

        private const int colored_circle_size = 416;

        //Time
        private const int full_animation_duration = 2950;

        private const int medium_ring_resize_duration = 360;
        private const int medium_ring_fade_duration = 420;

        private const int small_ring_animation_start_delay = 200;
        private const int small_ring_resize_duration = 250;
        private const int small_ring_fade_duration = 650;

        private const int text_appear_delay = 200;
        private const int text_fade_duration = 700;
        private const int text_spacing_transform_duration = 1500;

        private const int bar_animation_duration = 700;
        private const int bar_resize_delay = 150;

        private const int big_ring_animation_start_delay = 2000;
        private const int big_ring_resize_duration = 500;
        private const int big_ring_foreground_resize_delay = 250;
        private const int big_ring_fade_duration = 600;

        private const int background_animation_start_time = 2250;
        private const int foreground_animation_start_time = 2300;

        private const int colored_curcle_rotation_delay = 150;
        private const int purple_circle_animation_start_time = 2250;
        private const int yellow_circle_animation_start_time = 2315;
        private const int blue_circle_animation_start_time = 2380;
        private const int pink_circle_animation_start_time = 2445;

        //Position
        private const int bar_start_offset = 80;
        private const int bar_end_offset = 120;
        private const int colored_circle_offset = 250;

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
            int duration;

            mediumRing.ResizeTo(medium_ring_size, medium_ring_resize_duration, Easing.InExpo);
            mediumRing.Foreground.Delay(medium_ring_resize_duration).ResizeTo(1, medium_ring_fade_duration, Easing.OutQuad);

            using (welcomeText.BeginDelayedSequence(text_appear_delay))
            {
                welcomeText.FadeIn(text_fade_duration);
                welcomeText.TransformSpacingTo(new Vector2(20, 0), text_spacing_transform_duration, Easing.Out);
            }

            using (smallRing.BeginDelayedSequence(small_ring_animation_start_delay, true))
            {
                smallRing.ResizeTo(small_ring_size, small_ring_resize_duration, Easing.InExpo);
                smallRing.Foreground.Delay(small_ring_resize_duration).ResizeTo(1, small_ring_fade_duration, Easing.OutQuad);
            }

            duration = bar_animation_duration - bar_resize_delay;
            using (barsContainer.BeginDelayedSequence(medium_ring_resize_duration, true))
            {
                foreach (var bar in barsContainer)
                {
                    bar.FadeIn();
                    bar.Delay(bar_resize_delay).ResizeWidthTo(0, duration, Easing.OutQuint);
                }

                barTopLeft.MoveTo(new Vector2(-bar_end_offset, -bar_end_offset), bar_animation_duration, Easing.OutQuint);
                barTopRight.MoveTo(new Vector2(bar_end_offset, -bar_end_offset), bar_animation_duration, Easing.OutQuint);
                barBottomLeft.MoveTo(new Vector2(-bar_end_offset, bar_end_offset), bar_animation_duration, Easing.OutQuint);
                barBottomRight.MoveTo(new Vector2(bar_end_offset, bar_end_offset), bar_animation_duration, Easing.OutQuint);
            }

            using (bigRing.BeginDelayedSequence(big_ring_animation_start_delay, true))
            {
                bigRing.ResizeTo(big_ring_size, big_ring_resize_duration, Easing.InOutQuint);
                bigRing.Foreground.Delay(big_ring_foreground_resize_delay).ResizeTo(1, big_ring_fade_duration, Easing.OutExpo);
            }

            duration = full_animation_duration - background_animation_start_time;
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
            using (purpleCircle.BeginDelayedSequence(purple_circle_animation_start_time))
            {
                purpleCircle.MoveToY((colored_circle_size - 2) / 2, duration, Easing.InOutQuad);
                purpleCircle.Delay(colored_curcle_rotation_delay).RotateTo(-180, duration - colored_curcle_rotation_delay, Easing.OutQuad);
                purpleCircle.ResizeTo(colored_circle_size - 2, duration, Easing.InOutQuad);
            }

            duration = full_animation_duration - yellow_circle_animation_start_time;
            using (yellowCircle.BeginDelayedSequence(yellow_circle_animation_start_time))
            {
                yellowCircle.MoveToY(-(colored_circle_size - 2) / 2, duration, Easing.InOutQuad);
                yellowCircle.Delay(colored_curcle_rotation_delay).RotateTo(-180, duration - colored_curcle_rotation_delay, Easing.OutQuad);
                yellowCircle.ResizeTo(colored_circle_size - 2, duration, Easing.InOutQuad);
            }

            duration = full_animation_duration - blue_circle_animation_start_time;
            using (blueCircle.BeginDelayedSequence(blue_circle_animation_start_time))
            {
                blueCircle.MoveToX(-(colored_circle_size - 2) / 2, duration, Easing.InOutQuad);
                blueCircle.Delay(colored_curcle_rotation_delay).RotateTo(-180, duration - colored_curcle_rotation_delay, Easing.OutQuad);
                blueCircle.ResizeTo(colored_circle_size - 2, duration, Easing.InOutQuad);
            }

            duration = full_animation_duration - pink_circle_animation_start_time;
            using (pinkCircle.BeginDelayedSequence(pink_circle_animation_start_time))
            {
                pinkCircle.MoveToX(colored_circle_size / 2, duration, Easing.InOutQuad);
                pinkCircle.Delay(colored_curcle_rotation_delay).RotateTo(-180, duration - colored_curcle_rotation_delay, Easing.OutQuad);
                pinkCircle.ResizeTo(colored_circle_size, duration, Easing.InOutQuad);
            }

            logo.Delay(3200).FadeIn(300);

            backgroundFill.Delay(3200).FadeOut();
            foregroundFill.Delay(3500).FadeOut();
        }

        private void setDefaults()
        {
            logo.Alpha = 0;

            welcomeText.Spacing = new Vector2(5);
            welcomeText.Alpha = 0;

            smallRing.Size = mediumRing.Size = bigRing.Size = Vector2.Zero;
            mediumRing.Foreground.Size = Vector2.One - medium_ring_thickness;
            smallRing.Foreground.Size = Vector2.One - small_ring_thickness;
            bigRing.Foreground.Size = Vector2.One - big_ring_thickness;

            barTopLeft.Size = barTopRight.Size = barBottomLeft.Size = barBottomRight.Size = bar_size;
            barTopLeft.Alpha = barTopRight.Alpha = barBottomLeft.Alpha = barBottomRight.Alpha = 0;
            barTopLeft.Position = new Vector2(-bar_start_offset, -bar_start_offset);
            barTopRight.Position = new Vector2(bar_start_offset, -bar_start_offset);
            barBottomLeft.Position = new Vector2(-bar_start_offset, bar_start_offset);
            barBottomRight.Position = new Vector2(bar_start_offset, bar_start_offset);

            backgroundFill.Rotation = foregroundFill.Rotation = 0;
            backgroundFill.Alpha = foregroundFill.Alpha = 1;
            backgroundFill.Height = foregroundFill.Width = 0;

            yellowCircle.Size = purpleCircle.Size = blueCircle.Size = pinkCircle.Size = Vector2.Zero;
            yellowCircle.Rotation = purpleCircle.Rotation = blueCircle.Rotation = pinkCircle.Rotation = 0;
            yellowCircle.Position = new Vector2(0, -colored_circle_offset);
            purpleCircle.Position = new Vector2(0, colored_circle_offset);
            blueCircle.Position = new Vector2(-colored_circle_offset, 0);
            pinkCircle.Position = new Vector2(colored_circle_offset, 0);
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
