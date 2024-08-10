// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over.
    /// </summary>
    public partial class LegacyComboCounter : CompositeDrawable, ISerialisableDrawable
    {
        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0 };

        private uint scheduledPopOutCurrentId;

        private const double big_pop_out_duration = 300;

        private const double small_pop_out_duration = 100;

        private const double fade_out_duration = 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        private const double rolling_duration = 20;

        private readonly Drawable popOutCount;

        private readonly Drawable displayedCountSpriteText;

        private int previousValue;

        private int displayedCount;

        private bool isRolling;

        private readonly Container counterContainer;

        public bool UsesFixedAnchor { get; set; }

        public LegacyComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Margin = new MarginPadding(10);

            Scale = new Vector2(1.28f);

            InternalChildren = new[]
            {
                counterContainer = new Container
                {
                    AlwaysPresent = true,
                    Children = new[]
                    {
                        popOutCount = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            Blending = BlendingParameters.Additive,
                            Anchor = Anchor.BottomLeft,
                            BypassAutoSizeAxes = Axes.Both,
                        },
                        displayedCountSpriteText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
                            Anchor = Anchor.BottomLeft,
                            BypassAutoSizeAxes = Axes.Both,
                        },
                    }
                }
            };
        }

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

                if (isRolling)
                    onDisplayedCountRolling(value);
                else if (displayedCount + 1 == value)
                    onDisplayedCountIncrement(value);
                else
                    onDisplayedCountChange(value);

                displayedCount = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.Combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((IHasText)displayedCountSpriteText).Text = formatCount(Current.Value);
            ((IHasText)popOutCount).Text = formatCount(Current.Value);

            Current.BindValueChanged(combo => updateCount(combo.NewValue == 0), true);

            updateLayout();
        }

        private void updateLayout()
        {
            const float font_height_ratio = 0.625f;
            const float vertical_offset = 9;

            displayedCountSpriteText.OriginPosition = new Vector2(0, font_height_ratio * displayedCountSpriteText.Height + vertical_offset);
            displayedCountSpriteText.Position = new Vector2(0, -(1 - font_height_ratio) * displayedCountSpriteText.Height + vertical_offset);

            popOutCount.OriginPosition = new Vector2(3, font_height_ratio * popOutCount.Height + vertical_offset); // In stable, the bigger pop out scales a bit to the left
            popOutCount.Position = new Vector2(0, -(1 - font_height_ratio) * popOutCount.Height + vertical_offset);

            counterContainer.Size = displayedCountSpriteText.Size;
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
                isRolling = false;
                DisplayedCount = prev;

                if (prev + 1 == Current.Value)
                    onCountIncrement(prev, Current.Value);
                else
                    onCountChange(Current.Value);
            }
            else
            {
                onCountRolling(displayedCount, Current.Value);
                isRolling = true;
            }
        }

        private void transformPopOut(int newValue)
        {
            ((IHasText)popOutCount).Text = formatCount(newValue);

            popOutCount.ScaleTo(1.56f)
                       .ScaleTo(1, big_pop_out_duration);

            popOutCount.FadeTo(0.6f)
                       .FadeOut(big_pop_out_duration);
        }

        private void transformNoPopOut(int newValue)
        {
            ((IHasText)displayedCountSpriteText).Text = formatCount(newValue);

            counterContainer.Size = displayedCountSpriteText.Size;

            displayedCountSpriteText.ScaleTo(1);
        }

        private void transformPopOutSmall(int newValue)
        {
            ((IHasText)displayedCountSpriteText).Text = formatCount(newValue);

            counterContainer.Size = displayedCountSpriteText.Size;

            displayedCountSpriteText.ScaleTo(1).Then()
                                    .ScaleTo(1.1f, small_pop_out_duration / 2, Easing.In).Then()
                                    .ScaleTo(1, small_pop_out_duration / 2, Easing.Out);
        }

        private void scheduledPopOutSmall(uint id)
        {
            // Too late; scheduled task invalidated
            if (id != scheduledPopOutCurrentId)
                return;

            DisplayedCount++;
        }

        private void onCountIncrement(int currentValue, int newValue)
        {
            scheduledPopOutCurrentId++;

            if (DisplayedCount < currentValue)
                DisplayedCount++;

            displayedCountSpriteText.Show();

            transformPopOut(newValue);

            uint newTaskId = scheduledPopOutCurrentId;

            Scheduler.AddDelayed(delegate
            {
                scheduledPopOutSmall(newTaskId);
            }, big_pop_out_duration - 140);
        }

        private void onCountRolling(int currentValue, int newValue)
        {
            scheduledPopOutCurrentId++;

            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (currentValue == 0 && newValue == 0)
                displayedCountSpriteText.FadeOut(fade_out_duration);

            transformRoll(currentValue, newValue);
        }

        private void onCountChange(int newValue)
        {
            scheduledPopOutCurrentId++;

            if (newValue == 0)
                displayedCountSpriteText.FadeOut();

            DisplayedCount = newValue;
        }

        private void onDisplayedCountRolling(int newValue)
        {
            if (newValue == 0)
                displayedCountSpriteText.FadeOut(fade_out_duration);
            else
                displayedCountSpriteText.Show();

            transformNoPopOut(newValue);
        }

        private void onDisplayedCountChange(int newValue)
        {
            displayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);
            transformNoPopOut(newValue);
        }

        private void onDisplayedCountIncrement(int newValue)
        {
            displayedCountSpriteText.Show();
            transformPopOutSmall(newValue);
        }

        private void transformRoll(int currentValue, int newValue) =>
            this.TransformTo(nameof(DisplayedCount), newValue, getProportionalDuration(currentValue, newValue));

        private string formatCount(int count) => $@"{count}x";

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * rolling_duration;
        }
    }
}
