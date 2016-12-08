//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Allocation;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class RollingCounter<T> : Container
    {
        /// <summary>
        /// Type of the Transform to use.
        /// </summary>
        /// <remarks>
        /// Must be a subclass of Transform<T>
        /// </remarks>
        protected virtual Type TransformType => typeof(Transform<T>);

        protected SpriteText DisplayedCountSpriteText;

        /// <summary>
        /// If true, the roll-up duration will be proportional to change in value.
        /// </summary>
        protected virtual bool IsRollingProportional => false;

        /// <summary>
        /// If IsRollingProportional = false, duration in milliseconds for the counter roll-up animation for each
        /// element; else duration in milliseconds for the counter roll-up animation in total.
        /// </summary>
        protected virtual double RollingDuration => 0;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        protected virtual EasingTypes RollingEasing => EasingTypes.Out;

        private T displayedCount;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual T DisplayedCount
        {
            get
            {
                return displayedCount;
            }
            protected set
            {
                if (EqualityComparer<T>.Default.Equals(displayedCount, value))
                    return;
                displayedCount = value;
                DisplayedCountSpriteText.Text = FormatCount(value);
            }
        }

        protected T prevCount;
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
                prevCount = count;
                count = value;
                if (IsLoaded)
                {
                    TransformCount(displayedCount, count);
                }
            }
        }

        public void Set(T value)
        {
            Count = value;
        }

        public abstract void Increment(T amount);

        protected float textSize;

        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                DisplayedCountSpriteText.TextSize = value;
            }
        }

        /// <summary>
        /// Skeleton of a numeric counter which value rolls over time.
        /// </summary>
        protected RollingCounter()
        {
            Children = new Drawable[]
            {
                DisplayedCountSpriteText = new SpriteText(),
            };

            TextSize = 40;
            AutoSizeAxes = Axes.Both;

            DisplayedCount = Count;

            DisplayedCountSpriteText.Text = FormatCount(count);
            DisplayedCountSpriteText.Anchor = Anchor;
            DisplayedCountSpriteText.Origin = Origin;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Flush(false, TransformType);
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
        /// Stops rollover animation, forcing the displayed count to be the actual count.
        /// </summary>
        public virtual void StopRolling()
        {
            Flush(false, TransformType);
            DisplayedCount = Count;
        }

        /// <summary>
        /// Resets count to default value.
        /// </summary>
        public virtual void ResetCount()
        {
            SetCountWithoutRolling(default(T));
        }

        /// <summary>
        /// Calculates the duration of the roll-up animation by using the difference between the current visible value
        /// and the new final value.
        /// </summary>
        /// <remarks>
        /// To be used in conjunction with IsRollingProportional = true.
        /// Unless a derived class needs to have a proportional rolling, it is not necessary to override this function.
        /// </remarks>
        /// <param name="currentValue">Current visible value.</param>
        /// <param name="newValue">New final value.</param>
        /// <returns>Calculated rollover duration in milliseconds.</returns>
        protected virtual double GetProportionalDuration(T currentValue, T newValue)
        {
            return RollingDuration;
        }

        /// <summary>
        /// Used to format counts.
        /// </summary>
        /// <param name="count">Count to format.</param>
        /// <returns>Count formatted as a string.</returns>
        protected virtual string FormatCount(T count)
        {
            return count.ToString();
        }

        /// <summary>
        /// Called when the count is updated to add a transformer that changes the value of the visible count (i.e.
        /// implement the rollover animation).
        /// </summary>
        /// <param name="currentValue">Count value before modification.</param>
        /// <param name="newValue">Expected count value after modification-</param>
        /// <seealso cref="TransformType"/>
        protected virtual void TransformCount(T currentValue, T newValue)
        {
            Debug.Assert(
                TransformType.IsSubclassOf(typeof(Transform<T>)) || TransformType == typeof(Transform<T>),
                @"transformType should be a subclass of Transform<T>."
            );

            TransformCount((Transform<T>)Activator.CreateInstance(TransformType), currentValue, newValue);
        }

        /// <summary>
        /// Intended to be used by TransformCount(T currentValue, T newValue).
        /// </summary>
        protected void TransformCount(Transform<T> transform, T currentValue, T newValue)
        {
            Type type = transform.GetType();

            Flush(false, type);

            if (RollingDuration < 1)
            {
                DisplayedCount = Count;
                return;
            }

            double rollingTotalDuration =
                IsRollingProportional
                    ? GetProportionalDuration(currentValue, newValue)
                    : RollingDuration;

            transform.StartTime = Time.Current;
            transform.EndTime = Time.Current + rollingTotalDuration;
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }
    }
}
