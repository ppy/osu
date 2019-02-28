// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.HUD
{
    public abstract class ComboCounter : Container
    {
        public BindableInt Current = new BindableInt
        {
            MinValue = 0,
        };

        public bool IsRolling { get; protected set; }

        protected SpriteText PopOutCount;

        protected virtual double PopOutDuration => 150;
        protected virtual float PopOutScale => 2.0f;
        protected virtual Easing PopOutEasing => Easing.None;
        protected virtual float PopOutInitialAlpha => 0.75f;

        protected virtual double FadeOutDuration => 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        protected virtual double RollingDuration => 20;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        protected Easing RollingEasing => Easing.None;

        protected SpriteText DisplayedCountSpriteText;

        private int previousValue;

        /// <summary>
        /// Base of all combo counters.
        /// </summary>
        protected ComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                DisplayedCountSpriteText = new OsuSpriteText
                {
                    Alpha = 0,
                },
                PopOutCount = new OsuSpriteText
                {
                    Alpha = 0,
                    Margin = new MarginPadding(0.05f),
                }
            };

            TextSize = 80;

            Current.ValueChanged += combo => updateCount(combo.NewValue == 0);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedCountSpriteText.Text = FormatCount(Current.Value);
            DisplayedCountSpriteText.Anchor = Anchor;
            DisplayedCountSpriteText.Origin = Origin;

            StopRolling();
        }

        private int displayedCount;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual int DisplayedCount
        {
            get => displayedCount;
            protected set
            {
                if (displayedCount.Equals(value))
                    return;

                updateDisplayedCount(displayedCount, value, IsRolling);
            }
        }

        private float textSize;

        public float TextSize
        {
            get => textSize;
            set
            {
                textSize = value;

                DisplayedCountSpriteText.Font = DisplayedCountSpriteText.Font.With(size: TextSize);
                PopOutCount.Font = PopOutCount.Font.With(size: TextSize);
            }
        }

        /// <summary>
        /// Increments the combo by an amount.
        /// </summary>
        /// <param name="amount"></param>
        public void Increment(int amount = 1)
        {
            Current.Value = Current.Value + amount;
        }

        /// <summary>
        /// Stops rollover animation, forcing the displayed count to be the actual count.
        /// </summary>
        public void StopRolling()
        {
            updateCount(false);
        }

        protected virtual string FormatCount(int count)
        {
            return count.ToString();
        }

        protected virtual void OnCountRolling(int currentValue, int newValue)
        {
            transformRoll(currentValue, newValue);
        }

        protected virtual void OnCountIncrement(int currentValue, int newValue)
        {
            DisplayedCount = newValue;
        }

        protected virtual void OnCountChange(int currentValue, int newValue)
        {
            DisplayedCount = newValue;
        }

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * RollingDuration;
        }

        private void updateDisplayedCount(int currentValue, int newValue, bool rolling)
        {
            displayedCount = newValue;
            if (rolling)
                OnDisplayedCountRolling(currentValue, newValue);
            else if (currentValue + 1 == newValue)
                OnDisplayedCountIncrement(newValue);
            else
                OnDisplayedCountChange(newValue);
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
                IsRolling = false;
                DisplayedCount = prev;

                if (prev + 1 == Current.Value)
                    OnCountIncrement(prev, Current.Value);
                else
                    OnCountChange(prev, Current.Value);
            }
            else
            {
                OnCountRolling(displayedCount, Current.Value);
                IsRolling = true;
            }
        }

        private void transformRoll(int currentValue, int newValue)
        {
            this.TransformTo(nameof(DisplayedCount), newValue, getProportionalDuration(currentValue, newValue), RollingEasing);
        }

        protected abstract void OnDisplayedCountRolling(int currentValue, int newValue);
        protected abstract void OnDisplayedCountIncrement(int newValue);
        protected abstract void OnDisplayedCountChange(int newValue);
    }
}
