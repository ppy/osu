// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class RollingCounter<T> : Container, IHasAccentColour
    {
        /// <summary>
        /// The current value.
        /// </summary>
        public Bindable<T> Current = new Bindable<T>();

        /// <summary>
        /// Type of the Transform to use.
        /// </summary>
        /// <remarks>
        /// Must be a subclass of Transform(T)
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
        protected virtual EasingTypes RollingEasing => EasingTypes.OutQuint;

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

        public abstract void Increment(T amount);

        private float textSize;

        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                DisplayedCountSpriteText.TextSize = value;
            }
        }

        public Color4 AccentColour
        {
            get { return DisplayedCountSpriteText.Colour; }
            set { DisplayedCountSpriteText.Colour = value; }
        }

        /// <summary>
        /// Skeleton of a numeric counter which value rolls over time.
        /// </summary>
        protected RollingCounter()
        {
            Children = new Drawable[]
            {
                DisplayedCountSpriteText = new OsuSpriteText
                {
                    Font = @"Venera"
                },
            };

            TextSize = 40;
            AutoSizeAxes = Axes.Both;

            DisplayedCount = Current;

            Current.ValueChanged += newValue =>
            {
                if (IsLoaded) TransformCount(displayedCount, newValue);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedCountSpriteText.Text = FormatCount(Current);
        }

        /// <summary>
        /// Sets count value, bypassing rollover animation.
        /// </summary>
        /// <param name="count">New count value.</param>
        public virtual void SetCountWithoutRolling(T count)
        {
            Current.Value = count;
            StopRolling();
        }

        /// <summary>
        /// Stops rollover animation, forcing the displayed count to be the actual count.
        /// </summary>
        public virtual void StopRolling()
        {
            Flush(false, TransformType);
            DisplayedCount = Current;
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
                DisplayedCount = Current;
                return;
            }

            double rollingTotalDuration =
                IsRollingProportional
                    ? GetProportionalDuration(currentValue, newValue)
                    : RollingDuration;

            transform.StartTime = TransformStartTime;
            transform.EndTime = TransformStartTime + rollingTotalDuration;
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }
    }
}
