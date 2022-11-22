// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Practice.PracticeOverlayComponents
{
    public class PracticeSegmentSliderComponent : CompositeDrawable
    {
        [Resolved(canBeNull: true)]
        private DrawableRuleset? drawableRuleset { get; set; }

        private SegmentSliderStart segmentSliderStart = null!;
        private SegmentSliderEnd segmentSliderEnd = null!;

        public Bindable<double> customStartTime = new Bindable<double>();
        public Bindable<double> customEndTime = new Bindable<double>();

        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01f
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            if (drawableRuleset != null)
            {
                Objects = drawableRuleset.Objects;
            }

            const float vertical_offset = 15;

            Padding = new MarginPadding(10);
            InternalChildren = new Drawable[]
            {
                segmentSliderStart = new SegmentSliderStart
                {
                    Current = customStart,
                    Depth = float.MinValue,
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                },
                segmentSliderEnd = new SegmentSliderEnd
                {
                    Current = customEnd,
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            customStart.ValueChanged += min =>
            {
                customStartTime.Value = lastHitTime * min.NewValue;
                segmentSliderEnd.Current.Value = Math.Max(min.NewValue + 0.01, segmentSliderEnd.Current.Value);
            };
            customEnd.ValueChanged += max =>
            {
                customEndTime.Value = lastHitTime * max.NewValue;
                segmentSliderStart.Current.Value = Math.Min(max.NewValue - 0.01, segmentSliderStart.Current.Value);
            };
        }

        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;
                lastHitTime = objects.LastOrDefault()?.GetEndTime() ?? 0;
                UpdateObjects();
            }
        }

        private double lastHitTime { get; set; }
        protected virtual void UpdateObjects() { }

        private class SegmentSliderStart : SegmentSlider
        {
            public SegmentSliderStart()
                : base("Start")
            {
            }

            public override LocalisableString TooltipText => Current.Value.ToString(@"0.## %");

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

        private class SegmentSliderEnd : SegmentSlider
        {
            public SegmentSliderEnd()
                : base("End")
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RightBox.Height = 6; // just to match the left bar height really
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
                base.ReceivePositionalInputAt(screenSpacePos)
                && screenSpacePos.X >= Nub.ScreenSpaceDrawQuad.TopLeft.X;
        }

        private class SegmentSlider : OsuSliderBar<double>
        {
            private readonly string defaultString;

            public override LocalisableString TooltipText => Current.IsDefault
                ? "End"
                : Current.Value.ToString(@"0.## %");

            protected SegmentSlider(string defaultString)
            {
                DisplayAsPercentage = true;
                this.defaultString = defaultString;
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return true; // Make sure only one nub shows hover effect at once.
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Nub.Width = Nub.HEIGHT * 2;
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
                    currentDisplay.Text = current.NewValue != Current.Default ? current.NewValue.ToString(@"0.## %") : defaultString;
                }, true);
            }

            [BackgroundDependencyLoader]
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

