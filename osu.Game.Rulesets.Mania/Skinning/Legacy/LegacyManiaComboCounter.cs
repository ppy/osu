// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyManiaComboCounter : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0 };

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual int DisplayedCount
        {
            get => displayedCount;
            private set
            {
                if (displayedCount.Equals(value))
                    return;

                displayedCountText.FadeTo(value == 0 ? 0 : 1);
                displayedCountText.Text = value.ToString(CultureInfo.InvariantCulture);
                counterContainer.Size = displayedCountText.Size;

                displayedCount = value;
            }
        }

        private int displayedCount;

        private int previousValue;

        private const double fade_out_duration = 100;
        private const double rolling_duration = 20;

        private Container counterContainer = null!;
        private LegacySpriteText popOutCountText = null!;
        private LegacySpriteText displayedCountText = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, ScoreProcessor scoreProcessor)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                counterContainer = new Container
                {
                    AlwaysPresent = true,
                    Children = new[]
                    {
                        popOutCountText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            Blending = BlendingParameters.Additive,
                            BypassAutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ComboBreakColour)?.Value ?? Color4.Red,
                        },
                        displayedCountText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
                            BypassAutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                }
            };

            Current.BindTo(scoreProcessor.Combo);
        }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; } = null!;

        private IBindable<ScrollingDirection> direction = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayedCountText.Text = popOutCountText.Text = Current.Value.ToString(CultureInfo.InvariantCulture);

            Current.BindValueChanged(combo => updateCount(combo.NewValue == 0), true);

            counterContainer.Size = displayedCountText.Size;

            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => updateAnchor());

            // two schedules are required so that updateAnchor is executed in the next frame,
            // which is when the combo counter receives its Y position by the default layout in LegacyManiaSkinTransformer.
            Schedule(() => Schedule(updateAnchor));
        }

        private void updateAnchor()
        {
            // if the anchor isn't a vertical center, set top or bottom anchor based on scroll direction
            if (Anchor.HasFlag(Anchor.y1))
                return;

            Anchor &= ~(Anchor.y0 | Anchor.y2);
            Anchor |= direction.Value == ScrollingDirection.Up ? Anchor.y2 : Anchor.y0;

            // change the sign of the Y coordinate in line with the scrolling direction.
            // i.e. if the user changes direction from down to up, the anchor is changed from top to bottom, and the Y is flipped from positive to negative here.
            Y = Math.Abs(Y) * (direction.Value == ScrollingDirection.Up ? -1 : 1);
        }

        private void updateCount(bool rolling)
        {
            int prev = previousValue;
            previousValue = Current.Value;

            if (!IsLoaded)
                return;

            if (!rolling)
            {
                FinishTransforms(false, nameof(DisplayedCount));

                if (prev + 1 == Current.Value)
                    onCountIncrement();
                else
                    onCountChange();
            }
            else
                onCountRolling();
        }

        private void onCountIncrement()
        {
            popOutCountText.Hide();

            DisplayedCount = Current.Value;
            displayedCountText.ScaleTo(new Vector2(1f, 1.4f))
                              .ScaleTo(new Vector2(1f), 300, Easing.Out)
                              .FadeIn(120);
        }

        private void onCountChange()
        {
            popOutCountText.Hide();

            if (Current.Value == 0)
                displayedCountText.FadeOut();

            DisplayedCount = Current.Value;

            displayedCountText.ScaleTo(1f);
        }

        private void onCountRolling()
        {
            if (DisplayedCount > 0)
            {
                popOutCountText.Text = DisplayedCount.ToString(CultureInfo.InvariantCulture);
                popOutCountText.FadeTo(0.8f).FadeOut(200)
                               .ScaleTo(1f).ScaleTo(4f, 200);

                displayedCountText.FadeTo(0.5f, 300);
            }

            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (DisplayedCount == 0 && Current.Value == 0)
                displayedCountText.FadeOut(fade_out_duration);

            this.TransformTo(nameof(DisplayedCount), Current.Value, getProportionalDuration(DisplayedCount, Current.Value));
        }

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * rolling_duration;
        }
    }
}
