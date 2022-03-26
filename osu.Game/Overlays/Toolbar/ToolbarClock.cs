// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarClock : CompositeDrawable
    {
        private const float hand_thickness = 2.2f;

        public ToolbarClock()
        {
            RelativeSizeAxes = Axes.Y;
            Width = 110;

            Padding = new MarginPadding(10);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new AnalogDisplay
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                new DigitalDisplay
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                }
            };
        }

        private class DigitalDisplay : ClockDisplay
        {
            private OsuSpriteText text;

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Y;
                Width = 50;

                InternalChildren = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Text = "00:00:00",
                    }
                };
            }

            protected override void UpdateDisplay(DateTimeOffset now)
            {
                text.Text = $"{now:HH:mm:ss}";
            }
        }

        private class AnalogDisplay : ClockDisplay
        {
            private Drawable hour;
            private Drawable minute;
            private Drawable second;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Size = new Vector2(30);

                InternalChildren = new[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    hour = new LargeHand(0.3f),
                    minute = new LargeHand(0.45f),
                    second = new SecondHand(),
                    new CentreCircle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }

            private class CentreCircle : CompositeDrawable
            {
                public CentreCircle()
                {
                    InternalChildren = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(hand_thickness),
                            Colour = Color4.Black,
                        },
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(hand_thickness * 0.7f),
                            Colour = Color4.White,
                        },
                    };
                }
            }

            private class SecondHand : CompositeDrawable
            {
                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    RelativeSizeAxes = Axes.X;
                    Width = 0.54f;

                    Height = hand_thickness / 2;
                    Anchor = Anchor.Centre;
                    Origin = Anchor.Custom;

                    OriginPosition = new Vector2(Height * 2, Height / 2);

                    InternalChildren = new Drawable[]
                    {
                        new Circle
                        {
                            Colour = colours.YellowDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                    };
                }
            }

            private class LargeHand : CompositeDrawable
            {
                public LargeHand(float length)
                {
                    Width = length;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Anchor = Anchor.Centre;
                    Origin = Anchor.CentreLeft;

                    Origin = Anchor.Custom;
                    OriginPosition = new Vector2(hand_thickness / 2); // offset x also, to ensure the centre of the line is centered on the face.
                    Height = hand_thickness;

                    InternalChildren = new Drawable[]
                    {
                        new Circle
                        {
                            Colour = colours.PurpleLight,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 0.5f,
                            BorderColour = colours.Purple,
                        },
                    };

                    RelativeSizeAxes = Axes.X;
                }
            }

            protected override void UpdateDisplay(DateTimeOffset now)
            {
                float secondFractional = now.Second / 60f;
                float minuteFractional = (now.Minute + secondFractional) / 60f;
                float hourFractional = ((minuteFractional + now.Hour) % 12) / 12f;

                updateRotation(hour, hourFractional);
                updateRotation(minute, minuteFractional);
                updateRotation(second, secondFractional);
            }

            private void updateRotation(Drawable hand, float fraction)
            {
                const float duration = 320;

                float rotation = fraction * 360 - 90;

                if (Math.Abs(hand.Rotation - rotation) > 180)
                    hand.RotateTo(rotation);
                else
                    hand.RotateTo(rotation, duration, Easing.OutElastic);
            }
        }

        private abstract class ClockDisplay : CompositeDrawable
        {
            private int? lastSecond;

            protected override void Update()
            {
                base.Update();

                var now = DateTimeOffset.Now;

                if (now.Second != lastSecond)
                {
                    lastSecond = now.Second;
                    UpdateDisplay(now);
                }
            }

            protected abstract void UpdateDisplay(DateTimeOffset now);
        }
    }
}
