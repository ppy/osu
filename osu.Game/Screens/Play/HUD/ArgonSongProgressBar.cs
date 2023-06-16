// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressBar : SliderBar<double>
    {
        public Action<double>? OnSeek { get; set; }

        // Parent will handle restricting the area of valid input.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private readonly float barHeight;

        private readonly RoundedBar playfieldBar;
        private readonly RoundedBar catchupBar;

        private readonly Box background;

        private readonly ColourInfo mainColour;
        private ColourInfo catchUpColour;

        public double StartTime
        {
            private get => CurrentNumber.MinValue;
            set => CurrentNumber.MinValue = value;
        }

        public double EndTime
        {
            private get => CurrentNumber.MaxValue;
            set => CurrentNumber.MaxValue = value;
        }

        public double CurrentTime
        {
            private get => CurrentNumber.Value;
            set => CurrentNumber.Value = value;
        }

        public double TrackTime
        {
            private get => currentTrackTime.Value;
            set => currentTrackTime.Value = value;
        }

        private double length => EndTime - StartTime;

        private readonly BindableNumber<double> currentTrackTime;

        public bool Interactive { get; set; }

        public ArgonSongProgressBar(float barHeight)
        {
            currentTrackTime = new BindableDouble();
            setupAlternateValue();

            StartTime = 0;
            EndTime = 1;

            RelativeSizeAxes = Axes.X;
            Height = this.barHeight = barHeight;

            CornerRadius = 5;
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = OsuColour.Gray(0.2f),
                },
                catchupBar = new RoundedBar
                {
                    Name = "Audio bar",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    CornerRadius = 5,
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.Both
                },
                playfieldBar = new RoundedBar
                {
                    Name = "Playfield bar",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    CornerRadius = 5,
                    AccentColour = mainColour = OsuColour.Gray(0.9f),
                    RelativeSizeAxes = Axes.Both
                },
            };
        }

        private void setupAlternateValue()
        {
            CurrentNumber.MaxValueChanged += v => currentTrackTime.MaxValue = v;
            CurrentNumber.MinValueChanged += v => currentTrackTime.MinValue = v;
            CurrentNumber.PrecisionChanged += v => currentTrackTime.Precision = v;
        }

        private float normalizedReference
        {
            get
            {
                if (EndTime - StartTime == 0)
                    return 1;

                return (float)((TrackTime - StartTime) / length);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            catchUpColour = colours.BlueDark;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            background.FadeTo(0.3f, 200, Easing.In);
            playfieldBar.TransformTo(nameof(playfieldBar.AccentColour), mainColour, 200, Easing.In);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (Interactive)
                this.ResizeHeightTo(barHeight * 3.5f, 200, Easing.Out);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ResizeHeightTo(barHeight, 800, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override void UpdateValue(float value)
        {
            // Handled in Update
        }

        protected override void Update()
        {
            base.Update();

            playfieldBar.Length = (float)Interpolation.Lerp(playfieldBar.Length, NormalizedValue, Math.Clamp(Time.Elapsed / 40, 0, 1));
            catchupBar.Length = (float)Interpolation.Lerp(catchupBar.Length, normalizedReference, Math.Clamp(Time.Elapsed / 40, 0, 1));

            if (TrackTime < CurrentTime)
                ChangeChildDepth(catchupBar, -1);
            else
                ChangeChildDepth(catchupBar, 0);

            float timeDelta = (float)(Math.Abs(CurrentTime - TrackTime));

            const float colour_transition_threshold = 20000;

            catchupBar.AccentColour = Interpolation.ValueAt(
                Math.Min(timeDelta, colour_transition_threshold),
                mainColour,
                catchUpColour,
                0, colour_transition_threshold,
                Easing.OutQuint);

            catchupBar.Alpha = Math.Max(1, catchupBar.Length);
        }

        private ScheduledDelegate? scheduledSeek;

        protected override void OnUserChange(double value)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                if (Interactive)
                    OnSeek?.Invoke(value);
            });
        }

        private partial class RoundedBar : Container
        {
            private readonly Box fill;
            private readonly Container mask;
            private float length;

            public RoundedBar()
            {
                Masking = true;
                Children = new[]
                {
                    mask = new Container
                    {
                        Masking = true,
                        RelativeSizeAxes = Axes.Y,
                        Size = new Vector2(1),
                        Child = fill = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White
                        }
                    }
                };
            }

            public float Length
            {
                get => length;
                set
                {
                    length = value;
                    mask.Width = value * DrawWidth;
                }
            }

            public new float CornerRadius
            {
                get => base.CornerRadius;
                set
                {
                    base.CornerRadius = value;
                    mask.CornerRadius = value;
                }
            }

            public ColourInfo AccentColour
            {
                get => fill.Colour;
                set => fill.Colour = value;
            }
        }
    }
}
