// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedRangeSlider : CompositeDrawable
    {
        private readonly LocalisableString label;

        private readonly BindableNumberWithCurrent<double> lowerBound = new BindableNumberWithCurrent<double>();

        /// <summary>
        /// The lower limiting value.
        /// </summary>
        public Bindable<double> LowerBound
        {
            get => lowerBound.Current;
            set => lowerBound.Current = value;
        }

        private readonly BindableNumberWithCurrent<double> upperBound = new BindableNumberWithCurrent<double>();

        /// <summary>
        /// The upper limiting value.
        /// </summary>
        public Bindable<double> UpperBound
        {
            get => upperBound.Current;
            set => upperBound.Current = value;
        }

        public float NubWidth { get; init; }

        /// <summary>
        /// Minimum difference between the lower bound and higher bound
        /// </summary>
        public float MinRange
        {
            set => minRange = value;
        }

        /// <summary>
        /// Lower bound display for when it is set to its default value.
        /// </summary>
        public string DefaultStringLowerBound { get; init; } = string.Empty;

        /// <summary>
        /// Upper bound display for when it is set to its default value.
        /// </summary>
        public string DefaultStringUpperBound { get; init; } = string.Empty;

        public LocalisableString DefaultTooltipLowerBound { get; init; } = string.Empty;

        public LocalisableString DefaultTooltipUpperBound { get; init; } = string.Empty;

        public string TooltipSuffix { get; init; } = string.Empty;

        private float minRange = 0.1f;

        protected Container SliderContainer { get; private set; } = null!;

        protected BoundSliderBar LowerBoundSlider { get; private set; } = null!;
        protected BoundSliderBar UpperBoundSlider { get; private set; } = null!;

        protected Vector2 ScreenSpaceHalfwayPoint
        {
            get
            {
                var lowerSS = LowerBoundSlider.Nub.ScreenSpaceDrawQuad.TopLeft;
                var upperSS = UpperBoundSlider.Nub.ScreenSpaceDrawQuad.TopLeft;

                return lowerSS + (upperSS - lowerSS) / 2;
            }
        }

        public ShearedRangeSlider(LocalisableString label)
        {
            this.label = label;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = ShearedNub.HEIGHT;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                },
                Content = new[]
                {
                    new[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Masking = true,
                            CornerRadius = 5f,
                            Shear = OsuGame.SHEAR,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background3,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = label,
                                    Shear = -OsuGame.SHEAR,
                                    Margin = new MarginPadding { Horizontal = 12, Vertical = 5 },
                                    Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                },
                            },
                        },
                        SliderContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = -10 },
                            Children = new[]
                            {
                                UpperBoundSlider = CreateBoundSlider(true).With(d =>
                                {
                                    d.KeyboardStep = 0.1f;
                                    d.RelativeSizeAxes = Axes.X;
                                    d.TooltipSuffix = TooltipSuffix;
                                    d.DefaultString = DefaultStringUpperBound;
                                    d.DefaultTooltip = DefaultTooltipUpperBound;
                                    d.NubWidth = NubWidth;
                                    d.Current = upperBound;
                                }),
                                LowerBoundSlider = CreateBoundSlider(false).With(d =>
                                {
                                    d.KeyboardStep = 0.1f;
                                    d.RelativeSizeAxes = Axes.X;
                                    d.TooltipSuffix = TooltipSuffix;
                                    d.DefaultString = DefaultStringLowerBound;
                                    d.DefaultTooltip = DefaultTooltipLowerBound;
                                    d.NubWidth = NubWidth;
                                    d.Current = lowerBound;
                                }),
                                UpperBoundSlider.Nub.CreateProxy(),
                                LowerBoundSlider.Nub.CreateProxy(),
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LowerBoundSlider.Current.ValueChanged += min => UpperBoundSlider.Current.Value = Math.Max(min.NewValue + minRange, UpperBoundSlider.Current.Value);
            UpperBoundSlider.Current.ValueChanged += max => LowerBoundSlider.Current.Value = Math.Min(max.NewValue - minRange, LowerBoundSlider.Current.Value);
        }

        protected virtual BoundSliderBar CreateBoundSlider(bool isUpper) => new BoundSliderBar(this, isUpper);

        protected partial class BoundSliderBar : ShearedSliderBar<double>
        {
            private readonly ShearedRangeSlider rangeSlider;
            private readonly bool isUpper;

            public new float NormalizedValue => base.NormalizedValue;

            public new ShearedNub Nub => base.Nub;

            public string? DefaultString;
            public LocalisableString? DefaultTooltip;
            public string? TooltipSuffix;

            public float NubWidth { get; set; } = ShearedNub.HEIGHT;

            public override LocalisableString TooltipText =>
                (Current.IsDefault ? DefaultTooltip : Current.Value.ToString($@"0.## {TooltipSuffix}")) ?? Current.Value.ToString($@"0.## {TooltipSuffix}");

            protected OsuSpriteText NubText { get; private set; } = null!;

            public override bool AcceptsFocus => false;

            public BoundSliderBar(ShearedRangeSlider rangeSlider, bool isUpper)
            {
                this.rangeSlider = rangeSlider;
                this.isUpper = isUpper;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Nub.Width = NubWidth;
                RangePadding = Nub.Width / 2;

                Nub.Add(NubText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = -3,
                    UseFullGlyphHeight = false,
                    Colour = OsuColour.ForegroundTextColourFor(colourProvider.Light1),
                    Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                });

                AccentColour = colourProvider.Highlight1.Darken(0.1f);
                Nub.AccentColour = colourProvider.Highlight1;
                Nub.GlowingAccentColour = colourProvider.Highlight1;
                Nub.GlowColour = colourProvider.Highlight1;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (!isUpper)
                {
                    AccentColour = BackgroundColour;
                    BackgroundColour = Color4.Transparent;
                }

                Current.BindValueChanged(current => UpdateDisplay(current.NewValue), true);
                FinishTransforms(true);
            }

            protected virtual void UpdateDisplay(double value)
            {
                string defaultString = DefaultString ?? value.ToString("N1");
                NubText.Text = Current.IsDefault ? defaultString : value.ToString("N1");
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                if (isUpper)
                    return base.ReceivePositionalInputAt(screenSpacePos) && screenSpacePos.X > rangeSlider.ScreenSpaceHalfwayPoint.X;

                return base.ReceivePositionalInputAt(screenSpacePos) && screenSpacePos.X <= rangeSlider.ScreenSpaceHalfwayPoint.X;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (isUpper)
                {
                    // Only draw left box where required to avoid masking bleed issues.
                    LeftBox.X = ToParentSpace(ToLocalSpace(rangeSlider.LowerBoundSlider.Nub.ScreenSpaceDrawQuad.Centre)).X;
                    LeftBox.Size -= new Vector2(LeftBox.X, 0);
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return true; // Make sure only one nub shows hover effect at once.
            }
        }
    }
}
