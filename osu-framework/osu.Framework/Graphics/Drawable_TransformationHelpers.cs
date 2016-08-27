//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using System.Diagnostics;
using OpenTK;
using osu.Framework.Graphics.Transformations;

namespace osu.Framework.Graphics
{
    public partial class Drawable : IDisposable
    {
        private double transformationDelay;

        public void ClearTransformations()
        {
            Transformations.Clear();
            DelayReset();
        }

        public Drawable Delay(double duration, bool propagateChildren = false)
        {
            if (duration == 0) return this;

            transformationDelay += duration;
            if (propagateChildren)
                Children.ForEach(c => c.Delay(duration, propagateChildren));
            return this;
        }

        public Drawable DelayReset()
        {
            Delay(-transformationDelay);
            Children.ForEach(c => c.DelayReset());
            return this;
        }

        public void Loop(int delay = 0)
        {
            Transformations.ForEach(t =>
            {
                t.Loop = true;
                t.LoopDelay = Math.Max(0, transformationDelay + delay - t.Duration);
            });
        }

        /// <summary>
        /// Make this drawable automatically clean itself up after all transformations have finished playing.
        /// Can be delayed using Delay().
        /// </summary>
        public Drawable Expire(bool calculateLifetimeStart = false)
        {
            //expiry should happen either at the end of the last transformation or using the current sequence delay (whichever is highest).
            double max = Time + transformationDelay;
            foreach (Transformation t in Transformations)
                if (t.Time2 > max) max = t.Time2 + 1; //adding 1ms here ensures we can expire on the current frame without issue.
            LifetimeEnd = max;

            if (calculateLifetimeStart)
            {
                double min = double.MaxValue;
                foreach (Transformation t in Transformations)
                    if (t.Time1 < min) min = t.Time1;
                LifetimeStart = min < Int32.MaxValue ? min : Int32.MinValue;
            }

            return this;
        }

        public void TimeWarp(double change)
        {
            if (change == 0)
                return;

            foreach (Transformation t in Transformations)
            {
                t.Time1 += change;
                t.Time2 += change;
            }
        }

        /// <summary>
        /// Hide sprite instantly.
        /// </summary>
        /// <returns></returns>
        public virtual void Hide()
        {
            FadeOut(0);
        }

        /// <summary>
        /// Show sprite instantly.
        /// </summary>
        public virtual void Show()
        {
            FadeIn(0);
        }

        public Drawable FadeIn(double duration, EasingTypes easing = EasingTypes.None)
        {
            return FadeTo(1, duration, easing);
        }

        public Transformation FadeInFromZero(double duration)
        {
            if (transformationDelay == 0)
            {
                Alpha = 0;
                Transformations.RemoveAll(t => t.Type == TransformationType.Fade);
            }

            double startTime = Time + transformationDelay;

            Transformation tr = new Transformation(TransformationType.Fade, 0, 1, startTime, startTime + duration);
            Transformations.Add(tr);
            return tr;
        }

        public Drawable FadeOut(double duration, EasingTypes easing = EasingTypes.None)
        {
            return FadeTo(0, duration, easing);
        }

        public Transformation FadeOutFromOne(double duration)
        {
            if (transformationDelay == 0)
            {
                Alpha = 1;
                Transformations.RemoveAll(t => t.Type == TransformationType.Fade);
            }

            double startTime = Time + transformationDelay;

            Transformation tr =
                new Transformation(TransformationType.Fade, 1, 0, startTime, startTime + duration);
            Transformations.Add(tr);
            return tr;
        }

        #region Float-based helpers
        private Drawable transformFloatTo(float startValue, float newValue, double duration, EasingTypes easing, TransformationType transform)
        {
            if (transformationDelay == 0)
            {
                Transformations.RemoveAll(t => t.Type == transform);
                if (startValue == newValue)
                    return this;
            }
            else
                startValue = Transformations.FindLast(t => t.Type == transform)?.EndFloat ?? startValue;

            double startTime = Time + transformationDelay;

            Transformations.Add(new Transformation(transform, startValue, newValue, startTime, startTime + duration, easing));

            return this;
        }

        public Drawable FadeTo(float newAlpha, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                Alpha = newAlpha;
                return this;
            }

            return transformFloatTo(Alpha, newAlpha, duration, easing, TransformationType.Fade);
        }

        public Drawable ScaleTo(float newScale, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                Scale = newScale;
                return this;
            }

            return transformFloatTo(Scale, newScale, duration, easing, TransformationType.Scale);
        }

        public Drawable RotateTo(float newRotation, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                Rotation = newRotation;
                return this;
            }

            return transformFloatTo(Rotation, newRotation, duration, easing, TransformationType.Rotation);
        }

        [Obsolete]
        public Drawable MoveToX(float destination, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                Position = new Vector2(destination, Position.Y);
                return this;
            }

            return transformFloatTo(Position.X, destination, duration, easing, TransformationType.MovementX);
        }

        [Obsolete]
        public Drawable MoveToY(float destination, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                Position = new Vector2(Position.X, destination);
                return this;
            }

            return transformFloatTo(Position.Y, destination, duration, easing, TransformationType.MovementY);
        }
        #endregion

        #region Vector2-based helpers
        private Drawable transformVectorTo(Vector2 startValue, Vector2 newValue, double duration, EasingTypes easing, TransformationType transform)
        {
            if (transformationDelay == 0)
            {
                Transformations.RemoveAll(t => t.Type == transform);
                if (startValue == newValue)
                    return this;
            }
            else
                startValue = Transformations.FindLast(t => t.Type == transform)?.EndVector ?? startValue;

            double startTime = Time + transformationDelay;

            Transformations.Add(new Transformation(transform, startValue, newValue, startTime, startTime + duration, easing));

            return this;
        }

        public Drawable ScaleTo(Vector2 newScale, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                VectorScale = newScale;
                return this;
            }

            return transformVectorTo(VectorScale, newScale, duration, easing, TransformationType.VectorScale);
        }

        public Drawable MoveTo(Vector2 newPosition, double duration, EasingTypes easing = EasingTypes.None)
        {
            if (duration == 0)
            {
                Position = newPosition;
                return this;
            }

            return transformVectorTo(Position, newPosition, duration, easing, TransformationType.Movement);
        }

        public Drawable MoveToRelative(Vector2 offset, int duration, EasingTypes easing = EasingTypes.None)
        {
            return MoveTo(Transformations.FindLast(t => t.Type == TransformationType.Movement)?.EndVector ?? Position + offset, duration, easing);
        }
        #endregion

        #region Color4-based helpers
        public Drawable FadeColour(Color4 newColour, int duration, EasingTypes easing = EasingTypes.None)
        {
            Color4 startValue = Colour;
            if (transformationDelay == 0)
            {
                Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
                if (startValue == newColour)
                    return this;
            }
            else
                startValue = Transformations.FindLast(t => t.Type == TransformationType.Colour)?.EndColour ?? startValue;

            double startTime = Time + transformationDelay;

            Transformations.Add(new Transformation(startValue, newColour, startTime, startTime + duration, easing));

            return this;
        }

        public Drawable FlashColour(Color4 flashColour, int duration)
        {
            Debug.Assert(transformationDelay == 0, @"FlashColour doesn't support Delay() currently");

            Color4 startValue = Transformations.FindLast(t => t.Type == TransformationType.Colour)?.EndColour ?? Colour;
            Transformations.RemoveAll(t => t.Type == TransformationType.Colour);

            double startTime = Time + transformationDelay;

            Transformations.Add(new Transformation(flashColour, startValue, startTime, startTime + duration));

            return this;
        }
        #endregion
    }
}
