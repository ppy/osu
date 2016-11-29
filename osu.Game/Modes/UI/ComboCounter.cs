//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;

namespace osu.Game.Modes.UI
{
    public abstract class ComboCounter : Container
    {
        public bool IsRolling
        {
            get; protected set;
        }

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

        private ulong displayedCount;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual ulong DisplayedCount
        {
            get
            {
                return displayedCount;
            }
            protected set
            {
                if (displayedCount.Equals(value))
                    return;
                updateDisplayedCount(displayedCount, value, IsRolling);
            }
        }

        protected ulong count;

        /// <summary>
        /// Actual value of counter.
        /// </summary>
        public virtual ulong Count
        {
            get
            {
                return count;
            }
            set
            {
                updateCount(value);
            }
        }

        public void Increment(ulong amount = 1)
        {
            Count = Count + amount;
        }

        protected SpriteText DisplayedCountSpriteText;

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
        /// Base of all combo counters.
        /// </summary>
        protected ComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                DisplayedCountSpriteText = new SpriteText
                {
                    Alpha = 0,
                },
                PopOutCount = new SpriteText
                {
                    Alpha = 0,
                    Margin = new MarginPadding(0.05f),
                }
            };

            TextSize = 80;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedCountSpriteText.Text = FormatCount(Count);
            DisplayedCountSpriteText.Anchor = Anchor;
            DisplayedCountSpriteText.Origin = Origin;

            StopRolling();
        }

        /// <summary>
        /// Stops rollover animation, forcing the displayed count to be the actual count.
        /// </summary>
        public void StopRolling()
        {
            updateCount(Count);
        }

        /// <summary>
        /// Animates roll-back to 0.
        /// </summary>
        public void Roll()
        {
            Roll(0);
        }

        /// <summary>
        /// Animates roll-up/roll-back to an specific value.
        /// </summary>
        /// <param name="newValue">Target value.</param>
        public virtual void Roll(ulong newValue)
        {
            updateCount(newValue, true);
        }

        /// <summary>
        /// Resets count to default value.
        /// </summary>
        public virtual void ResetCount()
        {
            updateCount(0);
        }

        protected virtual string FormatCount(ulong count)
        {
            return count.ToString();
        }

        protected abstract void OnDisplayedCountRolling(ulong currentValue, ulong newValue);
        protected abstract void OnDisplayedCountIncrement(ulong newValue);
        protected abstract void OnDisplayedCountChange(ulong newValue);

        protected virtual void OnCountRolling(ulong currentValue, ulong newValue)
        {
            transformRoll(new TransformComboRoll(), currentValue, newValue);
        }

        protected virtual void OnCountIncrement(ulong currentValue, ulong newValue) {
            DisplayedCount = newValue;
        }

        protected virtual void OnCountChange(ulong currentValue, ulong newValue) {
            DisplayedCount = newValue;
        }

        private double getProportionalDuration(ulong currentValue, ulong newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * RollingDuration;
        }

        private void updateDisplayedCount(ulong currentValue, ulong newValue, bool rolling)
        {
            displayedCount = newValue;
            if (rolling)
                OnDisplayedCountRolling(currentValue, newValue);
            else if (currentValue + 1 == newValue)
                OnDisplayedCountIncrement(newValue);
            else
                OnDisplayedCountChange(newValue);
        }

        private void updateCount(ulong value, bool rolling = false)
        {
            ulong prevCount = count;
            count = value;

            if (!IsLoaded)
                return;

            if (!rolling)
            {
                Flush(false, typeof(TransformComboRoll));
                IsRolling = false;
                DisplayedCount = prevCount;

                if (prevCount + 1 == count)
                    OnCountIncrement(prevCount, count);
                else
                    OnCountChange(prevCount, count);
            }
            else
            {
                OnCountRolling(displayedCount, count);
                IsRolling = true;
            }
        }

        private void transformRoll(TransformComboRoll transform, ulong currentValue, ulong newValue)
        {
            Flush(false, typeof(TransformComboRoll));

            if (RollingDuration < 1)
            {
                DisplayedCount = Count;
                return;
            }

            transform.StartTime = Time.Current;
            transform.EndTime = Time.Current + getProportionalDuration(currentValue, newValue);
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }

        protected class TransformComboRoll : Transform<ulong>
        {
            protected override ulong CurrentValue
            {
                get
                {
                    double time = Time?.Current ?? 0;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return (ulong)Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as ComboCounter).DisplayedCount = CurrentValue;
            }
        }

        public void Set(ulong value)
        {
            if (value == 0)
                Roll();
            else
                Count = value;
        }
    }
}
