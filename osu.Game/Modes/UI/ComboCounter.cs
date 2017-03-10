// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Modes.UI
{
    public abstract class ComboCounter : Container
    {
        public BindableLong Current = new BindableLong
        {
            MinValue = 0,
        };

        public bool IsRolling { get; protected set; }

        protected SpriteText PopOutCount;

        protected virtual double PopOutDuration => 150;
        protected virtual float PopOutScale => 2.0f;
        protected virtual EasingTypes PopOutEasing => EasingTypes.None;
        protected virtual float PopOutInitialAlpha => 0.75f;

        protected virtual double FadeOutDuration => 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        protected virtual double RollingDuration => 20;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        protected EasingTypes RollingEasing => EasingTypes.None;

        protected SpriteText DisplayedCountSpriteText;

        private long previousValue;

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

            Current.ValueChanged += comboChanged;
        }

        private void comboChanged(object sender, System.EventArgs e)
        {
            updateCount(Current.Value == 0);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedCountSpriteText.Text = FormatCount(Current);
            DisplayedCountSpriteText.Anchor = Anchor;
            DisplayedCountSpriteText.Origin = Origin;

            StopRolling();
        }

        private long displayedCount;
        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual long DisplayedCount
        {
            get { return displayedCount; }
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
            get { return textSize; }
            set
            {
                textSize = value;
                DisplayedCountSpriteText.TextSize = TextSize;
                PopOutCount.TextSize = TextSize;
            }
        }

        /// <summary>
        /// Increments the combo by an amount.
        /// </summary>
        /// <param name="amount"></param>
        public void Increment(long amount = 1)
        {
            Current.Value = Current + amount;
        }

        /// <summary>
        /// Stops rollover animation, forcing the displayed count to be the actual count.
        /// </summary>
        public void StopRolling()
        {
            updateCount(false);
        }

        protected virtual string FormatCount(long count)
        {
            return count.ToString();
        }

        protected virtual void OnCountRolling(long currentValue, long newValue)
        {
            transformRoll(new TransformComboRoll(), currentValue, newValue);
        }

        protected virtual void OnCountIncrement(long currentValue, long newValue)
        {
            DisplayedCount = newValue;
        }

        protected virtual void OnCountChange(long currentValue, long newValue)
        {
            DisplayedCount = newValue;
        }

        private double getProportionalDuration(long currentValue, long newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * RollingDuration;
        }

        private void updateDisplayedCount(long currentValue, long newValue, bool rolling)
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
            long prev = previousValue;
            previousValue = Current;

            if (!IsLoaded)
                return;

            if (!rolling)
            {
                Flush(false, typeof(TransformComboRoll));
                IsRolling = false;
                DisplayedCount = prev;

                if (prev + 1 == Current)
                    OnCountIncrement(prev, Current);
                else
                    OnCountChange(prev, Current);
            }
            else
            {
                OnCountRolling(displayedCount, Current);
                IsRolling = true;
            }
        }

        private void transformRoll(TransformComboRoll transform, long currentValue, long newValue)
        {
            Flush(false, typeof(TransformComboRoll));

            if (RollingDuration < 1)
            {
                DisplayedCount = Current;
                return;
            }

            transform.StartTime = Time.Current;
            transform.EndTime = Time.Current + getProportionalDuration(currentValue, newValue);
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }

        protected class TransformComboRoll : Transform<long>
        {
            protected override long CurrentValue
            {
                get
                {
                    double time = Time?.Current ?? 0;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return (long)Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                ((ComboCounter)d).DisplayedCount = CurrentValue;
            }
        }

        protected abstract void OnDisplayedCountRolling(long currentValue, long newValue);
        protected abstract void OnDisplayedCountIncrement(long newValue);
        protected abstract void OnDisplayedCountChange(long newValue);
    }
}
