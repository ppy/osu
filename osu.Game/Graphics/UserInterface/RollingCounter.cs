//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Skeleton for a counter with a simple rollover animation.
    /// </summary>
    /// <typeparam name="T">Type of the actual counter.</typeparam>
    public abstract class RollingCounter<T> : Container
    {
        protected SpriteText countSpriteText;
        protected ulong RollingTotalDuration = 0;

        protected float textSize = 20.0f;
        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                updateTextSize();
            }
        }

        /// <summary>
        /// If true, each time the Count is updated, it will roll over from the current visible value.
        /// Else, it will roll over from the current count value.
        /// </summary>
        public bool IsRollingContinuous = true;

        /// <summary>
        /// If true, the rollover duration will be proportional to the counter.
        /// </summary>
        public bool IsRollingProportional = false;

        /// <summary>
        /// If IsRollingProportional = false, duration in milliseconds for the counter rollover animation for each element.
        /// If IsRollingProportional = true, duration in milliseconds for the counter rollover animation in total.
        /// </summary>
        public ulong RollingDuration = 0;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        public EasingTypes RollingEasing = EasingTypes.None;

        protected T visibleCount;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual T VisibleCount
        {
            get
            {
                return visibleCount;
            }
            protected set
            {
                if (visibleCount.Equals(value))
                    return;
                transformVisibleCount(visibleCount, value);
                visibleCount = value;
            }
        }

        protected T count;

        /// <summary>
        /// Actual value of counter.
        /// </summary>
        public virtual T Count
        {
            get
            {
                return count;
            }
            set
            {
                if (Clock != null)
                {
                    RollingTotalDuration = IsRollingProportional ? GetProportionalDuration(VisibleCount, value) : RollingDuration;
                    transformCount(IsRollingContinuous ? VisibleCount : count, value);
                }
                count = value;
            }
        }

        public override void Load()
        {
            base.Load();
            removeTransforms(typeof(Transform<T>));
            if (Count == null)
                ResetCount();
            VisibleCount = Count;
            Children = new Drawable[]
            {
                countSpriteText = new SpriteText
                {
                    Text = formatCount(Count),
                    TextSize = this.TextSize,
                    Anchor = this.Anchor,
                    Origin = this.Origin,
                },
            };
        }

        /// <summary>
        /// Calculates the duration of the rollover animation by using the difference between the current visible value and the new final value.
        /// </summary>
        /// <remarks>
        /// Intended to be used in conjunction with IsRolloverProportional = true.
        /// If you're sure your superclass won't never need to be proportional, then it is not necessary to override this function.
        /// </remarks>
        /// <param name="currentValue">Current visible value.</param>
        /// <param name="newValue">New final value.</param>
        /// <returns>Calculated rollover duration in milliseconds.</returns>
        protected virtual ulong GetProportionalDuration(T currentValue, T newValue)
        {
            return RollingDuration;
        }

        /// <summary>
        /// Used to format counts.
        /// </summary>
        /// <param name="count">Count to format.</param>
        /// <returns>Count formatted as a string.</returns>
        protected virtual string formatCount(T count)
        {
            return count.ToString();
        }

        /// <summary>
        /// Sets count value, bypassing rollover animation.
        /// </summary>
        /// <param name="count">New count value.</param>
        public virtual void SetCountWithoutRolling(T count)
        {
            Count = count;
            StopRolling();
        }

        /// <summary>
        /// Stops rollover animation, forcing the visible count to be the actual count.
        /// </summary>
        public virtual void StopRolling()
        {
            removeTransforms(typeof(Transform<T>));
            VisibleCount = Count;
        }

        /// <summary>
        /// Resets count to default value.
        /// </summary>
        public abstract void ResetCount();

        protected void updateTransforms(Type type)
        {
            foreach (ITransform t in Transforms.AliveItems)
                if (t.GetType().IsAssignableFrom(type))
                    t.Apply(this);
        }

        protected void removeTransforms(Type type)
        {
            Transforms.RemoveAll(t => t.GetType().IsSubclassOf(type));
        }

        /// <summary>
        /// Called when the count is updated to add a transformer that changes the value of the visible count (i.e. implement the rollover animation).
        /// </summary>
        /// <param name="currentValue">Count value before modification.</param>
        /// <param name="newValue">Expected count value after modification-</param>
        /// <remarks>
        /// Unless you need to set a custom animation according to the current or new value of the count, the recommended approach is to call
        /// transformCount(CustomTransformer(Clock), currentValue, newValue), where CustomTransformer is a custom Transformer related to the
        /// type T of the RolloverCounter.
        /// By using this approach, there is no need to check if the Clock is not null; this validation is done before adding the transformer.
        /// </remarks>
        protected abstract void transformCount(T currentValue, T newValue);

        /// <summary>
        /// Intended to be used by transformCount().
        /// </summary>
        /// <see cref="transformCount"/>
        protected void transformCount(Transform<T> transform, T currentValue, T newValue)
        {
            Type type = transform.GetType();

            updateTransforms(type);
            removeTransforms(type);

            if (Clock == null)
                return;

            if (RollingDuration == 0)
            {
                VisibleCount = Count;
                return;
            }

            transform.StartTime = Time;
            transform.EndTime = Time + RollingTotalDuration;
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }

        /// <summary>
        /// This procedure is called each time the visible count value is updated.
        /// Override to create custom animations.
        /// </summary>
        /// <param name="currentValue">Visible count value before modification.</param>
        /// <param name="newValue">Expected visible count value after modification-</param>
        protected virtual void transformVisibleCount(T currentValue, T newValue)
        {
            if (countSpriteText != null)
            {
                countSpriteText.Text = formatCount(newValue);
            }
        }

        protected virtual void updateTextSize()
        {
            if (countSpriteText != null)
                countSpriteText.TextSize = TextSize;
        }
    }
}
