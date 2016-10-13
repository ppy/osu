﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class StarCounter : RollingCounter<float>
    {
        protected override Type transformType => typeof(TransformStarCounter);

        protected Container starContainer;
        protected List<TextAwesome> stars = new List<TextAwesome>();

        /// <summary>
        /// Maximum amount of stars displayed.
        /// </summary>
        /// <remarks>
        /// This does not limit the counter value, but the amount of stars displayed.
        /// </remarks>
        public int MaxStars
        {
            get;
            protected set;
        }

        public ulong StarAnimationDuration = 500;
        public EasingTypes StarAnimationEasing = EasingTypes.OutElasticHalf;
        public ulong FadeDuration = 100;
        public float MinStarSize = 0.3f;
        public float MinStarAlpha = 0.5f;
        public int StarSize = 20;
        public int StarSpacing = 4;

        /// <summary>
        /// Shows a float count as stars. Used as star difficulty display.
        /// </summary>
        /// <param name="stars">Maximum amount of stars to display.</param>
        public StarCounter(int stars = 10)
        {
            IsRollingProportional = true;
            RollingDuration = 150;

            MaxStars = stars;

            Children = new Drawable[]
            {
                starContainer = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override ulong getProportionalDuration(float currentValue, float newValue)
        {
            return (ulong)(Math.Abs(currentValue - newValue) * RollingDuration);
        }

        public override void ResetCount()
        {
            Count = 0;
            StopRolling();
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            starContainer.Width = MaxStars * StarSize + Math.Max(MaxStars - 1, 0) * StarSpacing;
            starContainer.Height = StarSize;

            for (int i = 0; i < MaxStars; i++)
            {
                TextAwesome star = new TextAwesome
                {
                    Icon = FontAwesome.star,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    TextSize = StarSize,
                    Scale = new Vector2(MinStarSize),
                    Alpha = (i == 0) ? 1.0f : MinStarAlpha,
                    Position = new Vector2((StarSize + StarSpacing) * i + (StarSize + StarSpacing) / 2, 0),
                };

                //todo: user Container<T> once we have it.
                stars.Add(star);
                starContainer.Add(star);
            }

            ResetCount();
        }

        protected override void transformCount(float currentValue, float newValue)
        {
            transformStar((int)Math.Floor(currentValue), currentValue, currentValue < newValue);
            transformCount(new TransformStarCounter(Clock), currentValue, newValue);
        }

        protected void updateTransformStar(int i)
        {
            foreach (ITransform t in stars[i].Transforms.AliveItems)
                if (t.GetType() == typeof(TransformAlpha) || t.GetType() == typeof(TransformScaleVector))
                    t.Apply(stars[i]);

            stars[i].Transforms.RemoveAll(t =>
                t.GetType() == typeof(TransformScaleVector) || t.GetType() == typeof(TransformAlpha)
            );
        }

        protected void transformStarScale(int i, TransformScaleVector transform, bool isIncrement, double startTime)
        {
            transform.StartTime = startTime;
            transform.EndTime = transform.StartTime + StarAnimationDuration;
            transform.StartValue = stars[i].Scale;
            transform.EndValue = new Vector2(
                Interpolation.ValueAt(
                    Math.Min(Math.Max(i, Count), i + 1),
                    MinStarSize,
                    1.0f,
                    i,
                    i + 1
                )
            );
            transform.Easing = StarAnimationEasing;

            stars[i].Transforms.Add(transform);
        }

        protected void transformStarAlpha(int i, TransformAlpha transform, bool isIncrement, double startTime)
        {
            transform.StartTime = startTime;
            transform.EndTime = transform.StartTime + FadeDuration;
            transform.StartValue = stars[i].Alpha;
            transform.EndValue = i < Count ? 1.0f : MinStarAlpha;

            stars[i].Transforms.Add(transform);
        }


        protected void transformStar(int i, float value, bool isIncrement)
        {
            if (i >= MaxStars)
                return;

            if (Clock == null)
                return;

            // Calculate time where animation should had started
            double startTime = Time;
            // If incrementing, animation should had started when VisibleCount crossed start of star (i)
            if (isIncrement)
                startTime -= i == (int)Math.Floor(prevCount) ?
                    getProportionalDuration(prevCount, value) : getProportionalDuration(i, value);
            // If decrementing, animation should had started when VisibleCount crossed end of star (i + 1)
            else
                startTime -= i == (int)Math.Floor(prevCount) ?
                    getProportionalDuration(prevCount, value) : getProportionalDuration(i + 1, value);

            updateTransformStar(i);

            transformStarScale(i, new TransformScaleVector(Clock), isIncrement, startTime);
            transformStarAlpha(i, new TransformAlpha(Clock), isIncrement, startTime);
        }

        protected override void transformVisibleCount(float currentValue, float newValue)
        {
            // Detect increment that passes over an integer value
            if (Math.Ceiling(currentValue) <= Math.Floor(newValue))
                for (int i = (int)Math.Ceiling(currentValue); i <= Math.Floor(newValue); i++)
                    transformStar(i, newValue, true);

            // Detect decrement that passes over an integer value
            if (Math.Floor(currentValue) >= Math.Ceiling(newValue))
                for (int i = (int)Math.Floor(newValue); i < Math.Floor(currentValue); i++)
                    transformStar(i, newValue, false);
        }

        protected class TransformStarCounter : Transform<float>
        {
            public override float CurrentValue
            {
                get
                {
                    double time = Time;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as StarCounter).VisibleCount = CurrentValue;
            }

            public TransformStarCounter(IClock clock)
            : base(clock)
            {
            }
        }
    }
}
