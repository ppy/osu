// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards.Commands
{
    public abstract class StoryboardCommand<T> : IStoryboardCommand, IComparable<StoryboardCommand<T>>
    {
        public double StartTime { get; }
        public double EndTime { get; }

        public T StartValue { get; }
        public T EndValue { get; }
        public Easing Easing { get; }

        public double Duration => EndTime - StartTime;

        protected StoryboardCommand(double startTime, double endTime, T startValue, T endValue, Easing easing)
        {
            if (endTime < startTime)
                endTime = startTime;

            StartTime = startTime;
            StartValue = startValue;
            EndTime = endTime;
            EndValue = endValue;
            Easing = easing;
        }

        public abstract string PropertyName { get; }

        public abstract void ApplyInitialValue<TDrawable>(TDrawable d)
            where TDrawable : Drawable, IFlippable, IVectorScalable;

        public abstract TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            where TDrawable : Drawable, IFlippable, IVectorScalable;

        public int CompareTo(StoryboardCommand<T>? other)
        {
            if (other == null)
                return 1;

            int result = StartTime.CompareTo(other.StartTime);
            if (result != 0)
                return result;

            return EndTime.CompareTo(other.EndTime);
        }

        public override string ToString() => $"{StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";
    }
}
