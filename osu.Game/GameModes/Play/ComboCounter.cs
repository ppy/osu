//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.GameModes.Play
{
    public abstract class ComboCounter : Container
    {
        public bool IsRolling
        {
            get; protected set;
        }

        protected SpriteText PopOutSpriteText;

        protected virtual double PopOutDuration => 150;
        protected virtual float PopOutScale => 2.0f;
        protected virtual EasingTypes PopOutEasing => EasingTypes.None;
        protected virtual float PopOutInitialAlpha => 0.75f;

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

        protected ulong prevCount;
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
                PopOutSpriteText.TextSize = TextSize;
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
                PopOutSpriteText = new SpriteText
                {
                    Alpha = 0,
                }
            };

            TextSize = 80;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            DisplayedCountSpriteText.Text = FormatCount(Count);
            DisplayedCountSpriteText.Anchor = this.Anchor;
            DisplayedCountSpriteText.Origin = this.Origin;

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
            transformRoll(new TransformComboRoll(Clock), currentValue, newValue);
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
            prevCount = count;
            count = value;
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

            if (Clock == null)
                return;

            if (RollingDuration < 1)
            {
                DisplayedCount = Count;
                return;
            }

            transform.StartTime = Time;
            transform.EndTime = Time + getProportionalDuration(currentValue, newValue);
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }

        protected class TransformComboRoll : Transform<ulong>
        {
            public override ulong CurrentValue
            {
                get
                {
                    double time = Time;
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

            public TransformComboRoll(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
