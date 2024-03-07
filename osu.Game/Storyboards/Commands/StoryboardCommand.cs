// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public abstract class StoryboardCommand<T> : IStoryboardCommand, IComparable<StoryboardCommand<T>>
    {
        public double StartTime { get; }
        public double EndTime { get; }
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

        public Easing Easing { get; set; }
        public int LoopCount { get; set; }
        public double Delay { get; set; }

        public T StartValue;
        public T EndValue;

        /// <summary>
        /// Sets the value of the corresponding property in <see cref="Drawable"/> to the start value of this command.
        /// </summary>
        public abstract void SetInitialValue(Drawable d);

        /// <summary>
        /// Transforms a corresponding property in <see cref="Drawable"/> that corresponds to this command group with the specified parameters.
        /// </summary>
        public abstract TransformSequence<Drawable> ApplyTransform(Drawable d);

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
