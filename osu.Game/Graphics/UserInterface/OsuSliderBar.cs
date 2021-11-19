// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using JetBrains.Annotations;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Overlays;
using osu.Game.Utils;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuSliderBar<T> : SliderBar<T>, IHasTooltip, IHasAccentColour
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        private const int max_decimal_digits = 5;

        private Sample sample;
        private double lastSampleTime;
        private T lastSampleValue;

        protected readonly Nub Nub;
        private readonly Box leftBox;
        private readonly Box rightBox;
        private readonly Container nubContainer;

        public virtual LocalisableString TooltipText { get; private set; }

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage { get; set; }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                leftBox.Colour = value;
            }
        }

        private Colour4 backgroundColour;

        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                rightBox.Colour = value;
            }
        }

        public OsuSliderBar()
        {
            Height = Nub.HEIGHT;
            RangePadding = Nub.EXPANDED_SIZE / 2;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding { Horizontal = 2 },
                    Child = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Masking = true,
                        CornerRadius = 5f,
                        Children = new Drawable[]
                        {
                            leftBox = new Box
                            {
                                Height = 5,
                                EdgeSmoothness = new Vector2(0, 0.5f),
                                RelativeSizeAxes = Axes.None,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            rightBox = new Box
                            {
                                Height = 5,
                                EdgeSmoothness = new Vector2(0, 0.5f),
                                RelativeSizeAxes = Axes.None,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Alpha = 0.5f,
                            },
                        },
                    },
                },
                nubContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Nub = new Nub
                    {
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        Current = { Value = true }
                    },
                },
                new HoverClickSounds()
            };

            Current.DisabledChanged += disabled => { Alpha = disabled ? 0.3f : 1; };
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, [CanBeNull] OverlayColourProvider colourProvider, OsuColour colours)
        {
            sample = audio.Samples.Get(@"UI/notch-tick");
            AccentColour = colourProvider?.Highlight1 ?? colours.Pink;
            BackgroundColour = colourProvider?.Background5 ?? colours.Pink.Opacity(0.5f);
        }

        protected override void Update()
        {
            base.Update();

            nubContainer.Padding = new MarginPadding { Horizontal = RangePadding };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentNumber.BindValueChanged(current => updateTooltipText(current.NewValue), true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateGlow();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateGlow();
            base.OnHoverLost(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            updateGlow();
            base.OnDragEnd(e);
        }

        private void updateGlow()
        {
            Nub.Glowing = IsHovered || IsDragged;
        }

        protected override void OnUserChange(T value)
        {
            base.OnUserChange(value);
            playSample(value);
            updateTooltipText(value);
        }

        private void playSample(T value)
        {
            if (Clock == null || Clock.CurrentTime - lastSampleTime <= 30)
                return;

            if (value.Equals(lastSampleValue))
                return;

            lastSampleValue = value;
            lastSampleTime = Clock.CurrentTime;

            var channel = sample.GetChannel();

            channel.Frequency.Value = 0.99f + RNG.NextDouble(0.02f) + NormalizedValue * 0.2f;

            // intentionally pitched down, even when hitting max.
            if (NormalizedValue == 0 || NormalizedValue == 1)
                channel.Frequency.Value -= 0.5f;

            channel.Play();
        }

        private void updateTooltipText(T value)
        {
            if (CurrentNumber.IsInteger)
                TooltipText = value.ToInt32(NumberFormatInfo.InvariantInfo).ToString("N0");
            else
            {
                double floatValue = value.ToDouble(NumberFormatInfo.InvariantInfo);

                if (DisplayAsPercentage)
                {
                    TooltipText = floatValue.ToString("0%");
                }
                else
                {
                    decimal decimalPrecision = normalise(CurrentNumber.Precision.ToDecimal(NumberFormatInfo.InvariantInfo), max_decimal_digits);

                    // Find the number of significant digits (we could have less than 5 after normalize())
                    int significantDigits = FormatUtils.FindPrecision(decimalPrecision);

                    TooltipText = floatValue.ToString($"N{significantDigits}");
                }
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            leftBox.Scale = new Vector2(Math.Clamp(
                RangePadding + Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, DrawWidth), 1);
            rightBox.Scale = new Vector2(Math.Clamp(
                DrawWidth - Nub.DrawPosition.X - RangePadding - Nub.DrawWidth / 2, 0, DrawWidth), 1);
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX(value, 250, Easing.OutQuint);
        }

        /// <summary>
        /// Removes all non-significant digits, keeping at most a requested number of decimal digits.
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <param name="sd">The maximum number of decimal digits to keep. The final result may have fewer decimal digits than this value.</param>
        /// <returns>The normalised decimal.</returns>
        private decimal normalise(decimal d, int sd)
            => decimal.Parse(Math.Round(d, sd).ToString(string.Concat("0.", new string('#', sd)), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
