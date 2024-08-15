// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over.
    /// </summary>
    public abstract partial class LegacyComboCounter : CompositeDrawable, ISerialisableDrawable
    {
        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0 };

        private const double fade_out_duration = 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        private const double rolling_duration = 20;

        protected readonly LegacySpriteText PopOutCountText;
        protected readonly LegacySpriteText DisplayedCountText;

        private int previousValue;

        private int displayedCount;

        private bool isRolling;

        private readonly Container counterContainer;

        public bool UsesFixedAnchor { get; set; }

        protected LegacyComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                counterContainer = new Container
                {
                    AlwaysPresent = true,
                    Children = new[]
                    {
                        PopOutCountText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            Blending = BlendingParameters.Additive,
                            BypassAutoSizeAxes = Axes.Both,
                        },
                        DisplayedCountText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
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

            DisplayedCountText.Text = FormatCount(Current.Value);
            PopOutCountText.Text = FormatCount(Current.Value);

            Current.BindValueChanged(combo => updateCount(combo.NewValue == 0), true);

            counterContainer.Size = DisplayedCountText.Size;
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
                    OnCountIncrement();
                else
                    OnCountChange();
            }
            else
            {
                OnCountRolling();
                isRolling = true;
            }
        }

        /// <summary>
        /// Raised when the counter should display the new value with transitions.
        /// </summary>
        protected virtual void OnCountIncrement()
        {
            if (DisplayedCount < Current.Value - 1)
                DisplayedCount++;

            DisplayedCount++;
        }

        /// <summary>
        /// Raised when the counter should roll to the new combo value (usually roll back to zero).
        /// </summary>
        protected virtual void OnCountRolling()
        {
            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (DisplayedCount == 0 && Current.Value == 0)
                DisplayedCountText.FadeOut(fade_out_duration);

            transformRoll(DisplayedCount, Current.Value);
        }

        /// <summary>
        /// Raised when the counter should display the new combo value without any transitions.
        /// </summary>
        protected virtual void OnCountChange()
        {
            if (Current.Value == 0)
                DisplayedCountText.FadeOut();

            DisplayedCount = Current.Value;
        }

        private void onDisplayedCountRolling(int newValue)
        {
            if (newValue == 0)
                DisplayedCountText.FadeOut(fade_out_duration);

            DisplayedCountText.Text = FormatCount(newValue);
            counterContainer.Size = DisplayedCountText.Size;
        }

        private void onDisplayedCountChange(int newValue)
        {
            DisplayedCountText.FadeTo(newValue == 0 ? 0 : 1);
            DisplayedCountText.Text = FormatCount(newValue);

            counterContainer.Size = DisplayedCountText.Size;
        }

        private void onDisplayedCountIncrement(int newValue)
        {
            DisplayedCountText.Text = FormatCount(newValue);

            counterContainer.Size = DisplayedCountText.Size;
        }

        private void transformRoll(int currentValue, int newValue) =>
            this.TransformTo(nameof(DisplayedCount), newValue, getProportionalDuration(currentValue, newValue));

        protected virtual string FormatCount(int count) => $@"{count}";

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * rolling_duration;
        }
    }
}
