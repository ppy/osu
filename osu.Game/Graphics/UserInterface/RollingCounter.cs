using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Skeleton for a counter which value rolls-up in a lapse of time.
    /// </summary>
    /// <remarks>
    /// This class only abstracts the basics to roll-up a value in a lapse of time by using Transforms.
    /// In order to show a value, you must implement a way to display it, i.e., as a numeric counter or a bar.
    /// </remarks>
    /// <typeparam name="T">Type of the actual counter.</typeparam>
    public abstract class RollingCounter<T> : Container
    {
        /// <summary>
        /// Type of the Transform to use.
        /// </summary>
        /// <remarks>
        /// Must be a subclass of Transform<T>
        /// </remarks>
        protected virtual Type transformType => typeof(Transform<T>);

        protected ulong RollingTotalDuration = 0;

        /// <summary>
        /// If true, each time the Count is updated, it will roll over from the current visible value.
        /// Else, it will roll up from the current count value.
        /// </summary>
        public bool IsRollingContinuous = true;

        /// <summary>
        /// If true, the roll-up duration will be proportional to the counter.
        /// </summary>
        public bool IsRollingProportional = false;

        /// <summary>
        /// If IsRollingProportional = false, duration in milliseconds for the counter roll-up animation for each element.
        /// If IsRollingProportional = true, duration in milliseconds for the counter roll-up animation in total.
        /// </summary>
        public ulong RollingDuration = 0;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        public EasingTypes RollingEasing = EasingTypes.None;

        protected T prevVisibleCount;
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
                prevVisibleCount = visibleCount;
                if (visibleCount.Equals(value))
                    return;
                visibleCount = value;
                transformVisibleCount(prevVisibleCount, value);
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
                if (Clock != null)
                {
                    RollingTotalDuration =
                        IsRollingProportional
                            ? getProportionalDuration(VisibleCount, value)
                            : RollingDuration;
                    transformCount(IsRollingContinuous ? VisibleCount : count, value);
                }
            }
        }

        protected RollingCounter()
        {
            Debug.Assert(transformType.IsSubclassOf(typeof(Transform<T>)) || transformType == typeof(Transform<T>), @"transformType should be a subclass of Transform<T>.");
        }

        public override void Load()
        {
            base.Load();
            removeTransforms(transformType);
            if (Count == null)
                ResetCount();
            VisibleCount = Count;
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
            removeTransforms(transformType);
            VisibleCount = Count;
        }

        /// <summary>
        /// Resets count to default value.
        /// </summary>
        public abstract void ResetCount();

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
        protected virtual ulong getProportionalDuration(T currentValue, T newValue)
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

        protected void updateTransforms(Type type)
        {
            foreach (ITransform t in Transforms.AliveItems)
                if (t.GetType() == type)
                    t.Apply(this);
        }

        protected void removeTransforms(Type type)
        {
            Transforms.RemoveAll(t => t.GetType() == type);
        }

        /// <summary>
        /// Called when the count is updated to add a transformer that changes the value of the visible count (i.e.
        /// implement the rollover animation).
        /// </summary>
        /// <param name="currentValue">Count value before modification.</param>
        /// <param name="newValue">Expected count value after modification-</param>
        /// <remarks>
        /// Unless you need to set a custom animation according to the current or new value of the count, the
        /// recommended approach is to call transformCount(CustomTransformer(Clock), currentValue, newValue), where
        /// CustomTransformer is of type transformerType.
        /// By using this approach, there is no need to check if the Clock is not null; this validation is done before
        /// adding the transformer.
        /// </remarks>
        /// <seealso cref="transformType"/>
        protected virtual void transformCount(T currentValue, T newValue)
        {
            object[] parameters = { Clock };
            transformCount((Transform<T>)Activator.CreateInstance(transformType, parameters), currentValue, newValue);
        }

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
        protected abstract void transformVisibleCount(T currentValue, T newValue);
    }
}
