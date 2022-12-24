// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public partial class AnalogClockDisplay : ClockDisplay
    {
        private const float hand_thickness = 2.4f;

        private Drawable hour;
        private Drawable minute;
        private Drawable second;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(22);

            InternalChildren = new[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 2,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    },
                },
                hour = new LargeHand(0.34f),
                minute = new LargeHand(0.48f),
                second = new SecondHand(),
                new CentreCircle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
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

        private partial class CentreCircle : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(hand_thickness),
                        Colour = Color4.White,
                    },
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(hand_thickness * 0.7f),
                        Colour = colours.PinkLight,
                    },
                };
            }
        }

        private partial class SecondHand : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.X;
                Width = 0.66f;

                Height = hand_thickness * 0.7f;
                Anchor = Anchor.Centre;
                Origin = Anchor.Custom;

                OriginPosition = new Vector2(Height * 2, Height / 2);

                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        Colour = colours.PinkLight,
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }
        }

        private partial class LargeHand : CompositeDrawable
        {
            public LargeHand(float length)
            {
                Width = length;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Custom;

                OriginPosition = new Vector2(hand_thickness / 2); // offset x also, to ensure the centre of the line is centered on the face.

                Height = hand_thickness;

                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 0.7f,
                        BorderColour = colours.Gray2,
                    },
                };

                RelativeSizeAxes = Axes.X;
            }
        }
    }
}
