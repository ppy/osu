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
        protected Type transformType => typeof(TransformCombo);

        protected bool IsRolling = false;

        protected SpriteText PopOutSpriteText;

        protected virtual ulong PopOutDuration => 150;
        protected virtual float PopOutScale => 2.0f;
        protected virtual EasingTypes PopOutEasing => EasingTypes.None;
        protected virtual float PopOutInitialAlpha => 0.75f;

        /// <summary>
        /// If true, the roll-down duration will be proportional to the counter.
        /// </summary>
        protected virtual bool IsRollingProportional => true;

        /// <summary>
        /// If IsRollingProportional = false, duration in milliseconds for the counter roll-up animation for each
        /// element; else duration in milliseconds for the counter roll-up animation in total.
        /// </summary>
        protected virtual double RollingDuration => 20;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        protected EasingTypes RollingEasing => EasingTypes.None;

        private ulong prevVisibleCount;
        private ulong visibleCount;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual ulong VisibleCount
        {
            get
            {
                return visibleCount;
            }
            protected set
            {
                if (visibleCount.Equals(value))
                    return;
                prevVisibleCount = visibleCount;
                visibleCount = value;
                transformVisibleCount(prevVisibleCount, visibleCount, IsRolling);
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
                setCount(value);
            }
        }

        private void setCount(ulong value, bool rolling = false)
        {
            prevCount = count;
            count = value;
            if (IsLoaded)
            {
                transformCount(VisibleCount, prevCount, value, rolling);
            }
        }

        protected SpriteText CountSpriteText;

        private float textSize = 20.0f;
        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                CountSpriteText.TextSize = TextSize;
                PopOutSpriteText.TextSize = TextSize;
            }
        }

        /// <summary>
        /// Base of all combo counters.
        /// </summary>
        protected ComboCounter()
        {
            Children = new Drawable[]
            {
                CountSpriteText = new SpriteText
                {
                    Anchor = this.Anchor,
                    Origin = this.Origin,
                    Alpha = 0,
                },
                PopOutSpriteText = new SpriteText
                {
                    Alpha = 0,
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            CountSpriteText.Anchor = this.Anchor;
            CountSpriteText.Origin = this.Origin;

            StopRolling();
        }

        /// <summary>
        /// Stops rollover animation, forcing the visible count to be the actual count.
        /// </summary>
        public virtual void StopRolling()
        {
            Flush(false, typeof(TransformCombo));
            VisibleCount = Count;
        }

        /// <summary>
        /// Animates roll-up/roll-back to an specific value.
        /// </summary>
        /// <param name="newValue">Target value.</param>
        public virtual void Roll(ulong newValue = 0)
        {
            setCount(newValue, true);
        }

        /// <summary>
        /// Resets count to default value.
        /// </summary>
        public virtual void ResetCount()
        {
            Count = default(ulong);
        }

        protected double GetProportionalDuration(ulong currentValue, ulong newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : currentValue - newValue;
            return difference * RollingDuration;
        }

        protected virtual string FormatCount(ulong count)
        {
            return count.ToString();
        }

        protected abstract void OnCountRolling(ulong currentValue, ulong newValue);
        protected abstract void OnCountIncrement(ulong newValue);
        protected abstract void OnCountChange(ulong newValue);

        private void transformVisibleCount(ulong currentValue, ulong newValue, bool rolling)
        {
            if (rolling)
                OnCountRolling(currentValue, newValue);
            else if (currentValue + 1 == newValue)
                OnCountIncrement(newValue);
            else
                OnCountChange(newValue);
        }

        private void transformCount(
            ulong visibleValue,
            ulong currentValue,
            ulong newValue,
            bool rolling)
        {
            if (!rolling)
            {
                Flush(false, typeof(TransformCombo));
                IsRolling = false;

                VisibleCount = currentValue;
                VisibleCount = newValue;
            }
            else
            {
                IsRolling = true;
                transformRoll(new TransformCombo(Clock), visibleValue, newValue);
            }
        }

        private void transformRoll(TransformCombo transform, ulong currentValue, ulong newValue)
        {
            Flush(false, typeof(TransformCombo));

            if (Clock == null)
                return;

            if (RollingDuration == 0)
            {
                VisibleCount = Count;
                return;
            }

            double rollingTotalDuration =
                IsRollingProportional
                    ? GetProportionalDuration(currentValue, newValue)
                    : RollingDuration;

            transform.StartTime = Time;
            transform.EndTime = Time + rollingTotalDuration;
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }

        protected class TransformCombo : Transform<ulong>
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
                (d as ComboCounter).VisibleCount = CurrentValue;
            }

            public TransformCombo(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
