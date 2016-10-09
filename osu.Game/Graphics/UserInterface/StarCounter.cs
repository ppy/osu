//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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
    /// <summary>
    /// Shows a float count as stars. Used as star difficulty display.
    /// </summary>
    public class StarCounter : RollingCounter<float>
    {
        protected override Type transformType => typeof(TransformStarCounter);

        protected Container starContainer;
        protected List<TextAwesome> stars = new List<TextAwesome>();

        public ulong StarAnimationDuration = 500;
        public ulong FadeDuration = 100;
        public float MinStarSize = 0.3f;
        public float MinStarAlpha = 0.5f;
        public int MaxStars = 10;
        public int StarSize = 20;
        public int StarSpacing = 4;

        public StarCounter() : base()
        {
            IsRollingProportional = true;
            RollingDuration = 150;
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

        public override void Load()
        {
            base.Load();

            Children = new Drawable[]
            {
                starContainer = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = MaxStars * StarSize + Math.Max(MaxStars - 1, 0) * StarSpacing,
                    Height = StarSize,
                }
            };

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
                stars.Add(star);
                starContainer.Add(star);
            }

            ResetCount();
        }

        protected override void transformCount(float currentValue, float newValue)
        {
            transformStar((int)Math.Floor(currentValue), currentValue < newValue);
            transformCount(new TransformStarCounter(Clock), currentValue, newValue);
        }

        protected void updateTransformStar(int i)
        {
            foreach (ITransform t in stars[i].Transforms.AliveItems)
                if (t.GetType() == typeof(TransformAlpha) || t.GetType() == typeof(TransformScaleVector))
                    t.Apply(stars[i]);

            stars[i].Transforms.RemoveAll(t => t.GetType() == typeof(TransformScaleVector) || t.GetType() == typeof(TransformAlpha));
        }

        protected void transformStarScale(int i, TransformScaleVector transform, bool isIncrement, double startTime)
        {
            transform.StartTime = startTime;
            transform.EndTime = transform.StartTime + StarAnimationDuration;
            transform.StartValue = stars[i].Scale;
            transform.EndValue = new Vector2(Interpolation.ValueAt((isIncrement ? Math.Min(i + 1, Count) : Math.Max(i, Count)), MinStarSize, 1.0f, i, i + 1));
            transform.Easing = EasingTypes.OutElasticHalf;

            stars[i].Transforms.Add(transform);
        }

        protected void transformStarAlpha(int i, TransformAlpha transform, bool isIncrement, double startTime)
        {
            transform.StartTime = startTime;
            //if (!isIncrement)
                //transform.StartTime += StarAnimationDuration - FadeDuration;
            transform.EndTime = transform.StartTime + FadeDuration;
            transform.StartValue = stars[i].Alpha;
            transform.EndValue = i < Count ? 1.0f : MinStarAlpha;

            stars[i].Transforms.Add(transform);
        }


        protected void transformStar(int i, bool isIncrement)
        {
            if (Clock == null)
                return;

            // Calculate time where animation should had started
            double startTime = Time;
            // If incrementing, animation should had started when VisibleCount crossed start of star (i)
            if (isIncrement)
                startTime -= i == (int)Math.Floor(prevCount) ? getProportionalDuration(prevCount, VisibleCount) : getProportionalDuration(i, VisibleCount);
            // If decrementing, animation should had started when VisibleCount crossed end of star (i + 1)
            else
                startTime -= i == (int)Math.Floor(prevCount) ? getProportionalDuration(prevCount, VisibleCount) : getProportionalDuration(i + 1, VisibleCount);

            updateTransformStar(i);

            transformStarScale(i, new TransformScaleVector(Clock), isIncrement, startTime);
            transformStarAlpha(i, new TransformAlpha(Clock), isIncrement, startTime);
        }

        protected override void transformVisibleCount(float currentValue, float newValue)
        {
            // Detect increment that passes over an integer value
            if (Math.Ceiling(currentValue) <= Math.Floor(newValue))
                for (int i = (int)Math.Ceiling(currentValue); i <= Math.Floor(newValue); i++)
                    transformStar(i, true);

            // Detect decrement that passes over an integer value
            if (Math.Floor(currentValue) >= Math.Ceiling(newValue))
                for (int i = (int)Math.Floor(newValue); i < Math.Floor(currentValue); i++)
                    transformStar(i, false);
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
