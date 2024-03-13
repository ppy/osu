// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class RangeSlider : CompositeDrawable
    {
        /// <summary>
        /// The lower limiting value
        /// </summary>
        public Bindable<double> LowerBound
        {
            get => lowerBound.Current;
            set => lowerBound.Current = value;
        }

        /// <summary>
        /// The upper limiting value
        /// </summary>
        public Bindable<double> UpperBound
        {
            get => upperBound.Current;
            set => upperBound.Current = value;
        }

        /// <summary>
        /// Text that describes this RangeSlider's functionality
        /// </summary>
        public string Label
        {
            set => label.Text = value;
        }

        public float NubWidth
        {
            set => lowerBound.NubWidth = upperBound.NubWidth = value;
        }

        /// <summary>
        /// Minimum difference between the lower bound and higher bound
        /// </summary>
        public float MinRange
        {
            set => minRange = value;
        }

        /// <summary>
        /// lower bound display for when it is set to its default value
        /// </summary>
        public string DefaultStringLowerBound
        {
            set => lowerBound.DefaultString = value;
        }

        /// <summary>
        /// upper bound display for when it is set to its default value
        /// </summary>
        public string DefaultStringUpperBound
        {
            set => upperBound.DefaultString = value;
        }

        public LocalisableString DefaultTooltipLowerBound
        {
            set => lowerBound.DefaultTooltip = value;
        }

        public LocalisableString DefaultTooltipUpperBound
        {
            set => upperBound.DefaultTooltip = value;
        }

        public string TooltipSuffix
        {
            set => upperBound.TooltipSuffix = lowerBound.TooltipSuffix = value;
        }

        private float minRange = 0.1f;

        private readonly OsuSpriteText label;

        private readonly LowerBoundSlider lowerBound;
        private readonly UpperBoundSlider upperBound;

        public RangeSlider()
        {
            const float vertical_offset = 13;

            InternalChildren = new Drawable[]
            {
                label = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 14),
                },
                upperBound = new UpperBoundSlider
                {
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                },
                lowerBound = new LowerBoundSlider
                {
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            lowerBound.Current.ValueChanged += min => upperBound.Current.Value = Math.Max(min.NewValue + minRange, upperBound.Current.Value);
            upperBound.Current.ValueChanged += max => lowerBound.Current.Value = Math.Min(max.NewValue - minRange, lowerBound.Current.Value);
        }

        private partial class LowerBoundSlider : BoundSlider
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                LeftBox.Height = 6; // hide any colour bleeding from overlap

                AccentColour = BackgroundColour;
                BackgroundColour = Color4.Transparent;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
                base.ReceivePositionalInputAt(screenSpacePos)
                && screenSpacePos.X <= Nub.ScreenSpaceDrawQuad.TopRight.X;
        }

        private partial class UpperBoundSlider : BoundSlider
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                RightBox.Height = 6; // just to match the left bar height really
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
                base.ReceivePositionalInputAt(screenSpacePos)
                && screenSpacePos.X >= Nub.ScreenSpaceDrawQuad.TopLeft.X;
        }

        protected partial class BoundSlider : RoundedSliderBar<double>
        {
            public string? DefaultString;
            public LocalisableString? DefaultTooltip;
            public string? TooltipSuffix;
            public float NubWidth { get; set; } = Nub.HEIGHT;

            public override LocalisableString TooltipText =>
                (Current.IsDefault ? DefaultTooltip : Current.Value.ToString($@"0.## {TooltipSuffix}")) ?? Current.Value.ToString($@"0.## {TooltipSuffix}");

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return true; // Make sure only one nub shows hover effect at once.
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Nub.Width = NubWidth;
                RangePadding = Nub.Width / 2;

                OsuSpriteText currentDisplay;

                Nub.Add(currentDisplay = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -0.5f,
                    Colour = Color4.White,
                    Font = OsuFont.Torus.With(size: 10),
                });

                Current.BindValueChanged(current =>
                {
                    currentDisplay.Text = (current.NewValue != Current.Default ? current.NewValue.ToString("N1") : DefaultString) ?? current.NewValue.ToString("N1");
                }, true);
            }

            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider? colourProvider)
            {
                if (colourProvider == null) return;

                AccentColour = colourProvider.Background2;
                Nub.AccentColour = colourProvider.Background2;
                Nub.GlowingAccentColour = colourProvider.Background1;
                Nub.GlowColour = colourProvider.Background2;
            }
        }
    }
}
