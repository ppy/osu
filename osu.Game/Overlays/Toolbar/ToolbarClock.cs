// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarClock : CompositeDrawable
    {
        private Bindable<ToolbarClockDisplayMode> clockDisplayMode;

        private DigitalDisplay digital;
        private AnalogDisplay analog;

        private const float hand_thickness = 2.4f;

        public ToolbarClock()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Padding = new MarginPadding(5);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            clockDisplayMode = config.GetBindable<ToolbarClockDisplayMode>(OsuSetting.ToolbarClockDisplayMode);

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    analog = new AnalogDisplay
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    digital = new DigitalDisplay
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            clockDisplayMode.BindValueChanged(displayMode =>
            {
                bool showAnalog = displayMode.NewValue == ToolbarClockDisplayMode.Analog || displayMode.NewValue == ToolbarClockDisplayMode.Full;
                bool showDigital = displayMode.NewValue != ToolbarClockDisplayMode.Analog;
                bool showRuntime = displayMode.NewValue == ToolbarClockDisplayMode.DigitalWithRuntime || displayMode.NewValue == ToolbarClockDisplayMode.Full;

                digital.FadeTo(showDigital ? 1 : 0);
                digital.ShowRuntime = showRuntime;

                analog.FadeTo(showAnalog ? 1 : 0);
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            cycleDisplayMode();
            return true;
        }

        private void cycleDisplayMode()
        {
            switch (clockDisplayMode.Value)
            {
                case ToolbarClockDisplayMode.Analog:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.Full;
                    break;

                case ToolbarClockDisplayMode.Digital:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.Analog;
                    break;

                case ToolbarClockDisplayMode.DigitalWithRuntime:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.Digital;
                    break;

                case ToolbarClockDisplayMode.Full:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.DigitalWithRuntime;
                    break;
            }
        }

        private class DigitalDisplay : ClockDisplay
        {
            private OsuSpriteText realTime;
            private OsuSpriteText gameTime;

            private bool showRuntime = true;

            public bool ShowRuntime
            {
                get => showRuntime;
                set
                {
                    if (showRuntime == value)
                        return;

                    showRuntime = value;
                    updateMetrics();
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    realTime = new OsuSpriteText(),
                    gameTime = new OsuSpriteText
                    {
                        Y = 14,
                        Colour = colours.PinkLight,
                        Scale = new Vector2(0.6f)
                    }
                };

                updateMetrics();
            }

            protected override void UpdateDisplay(DateTimeOffset now)
            {
                realTime.Text = $"{now:HH:mm:ss}";
                gameTime.Text = $"running {new TimeSpan(TimeSpan.TicksPerSecond * (int)(Clock.CurrentTime / 1000)):c}";
            }

            private void updateMetrics()
            {
                Width = showRuntime ? 66 : 45; // Allows for space for game time up to 99 days (in the padding area since this is quite rare).
                gameTime.FadeTo(showRuntime ? 1 : 0);
            }
        }

        private class AnalogDisplay : ClockDisplay
        {
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

            private class CentreCircle : CompositeDrawable
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

            private class SecondHand : CompositeDrawable
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
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 0.7f,
                            BorderColour = colours.Gray2,
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
