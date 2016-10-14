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

namespace osu.Game.Graphics.UserInterface
{
    public abstract class RollingCounter<T> : AutoSizeContainer
    {
        /// <summary>
        /// Type of the Transform to use.
        /// </summary>
        /// <remarks>
        /// Must be a subclass of Transform<T>
        /// </remarks>
        protected virtual Type transformType => typeof(Transform<T>);

        protected double rollingTotalDuration = 0;

        protected SpriteText countSpriteText;

        /// <summary>
        /// If true, the roll-up duration will be proportional to change in value.
        /// </summary>
        public bool IsRollingProportional = false;

        /// <summary>
        /// If IsRollingProportional = false, duration in milliseconds for the counter roll-up animation for each
        /// element; else duration in milliseconds for the counter roll-up animation in total.
        /// </summary>
        public double RollingDuration = 0;

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
                if (visibleCount.Equals(value))
                    return;
                visibleCount = value;
                countSpriteText.Text = formatCount(value);
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
                    rollingTotalDuration =
                        IsRollingProportional
                            ? getProportionalDuration(visibleCount, value)
                            : RollingDuration;
                    transformCount(visibleCount, count);
                }
            }
        }

        protected float textSize = 20.0f;

        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                countSpriteText.TextSize = value;
            }
        }

        /// <summary>
        /// Skeleton of a numeric counter which value rolls over time.
        /// </summary>
        protected RollingCounter()
        {
            Debug.Assert(
                transformType.IsSubclassOf(typeof(Transform<T>)) || transformType == typeof(Transform<T>),
                @"transformType should be a subclass of Transform<T>."
            );

            Children = new Drawable[]
            {
                countSpriteText = new SpriteText
                {
                    Anchor = this.Anchor,
                    Origin = this.Origin,
                },
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            removeTransforms(transformType);

            VisibleCount = Count;

            countSpriteText.Text = formatCount(count);
            countSpriteText.Anchor = this.Anchor;
            countSpriteText.Origin = this.Origin;
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
        protected virtual double getProportionalDuration(T currentValue, T newValue)
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
            transform.EndTime = Time + rollingTotalDuration;
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }
    }
}
