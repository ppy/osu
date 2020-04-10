// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using osu.Framework.Lists;

namespace osu.Game.Lists
{
    /// <summary>
    /// Represents a list of intervals that can be used for whether a specific value falls into one of them.
    /// </summary>
    /// <typeparam name="T">The type of interval values.</typeparam>
    public class IntervalList<T> : IEnumerable<Interval<T>>
        where T : struct, IConvertible
    {
        private static readonly IComparer<T> type_comparer = Comparer<T>.Default;

        private readonly SortedList<Interval<T>> intervals = new SortedList<Interval<T>>((x, y) => type_comparer.Compare(x.Start, y.Start));
        private int nearestIndex;

        public Interval<T> this[int i]
        {
            get => intervals[i];
            set => intervals[i] = value;
        }

        /// <summary>
        /// Whether the provided value is in any interval added to this list.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        public bool IsInAnyInterval(T value)
        {
            if (intervals.Count == 0)
                return false;

            // Clamp the nearest index in case there were intervals
            // removed from the list causing the index to go out of range.
            nearestIndex = Math.Clamp(nearestIndex, 0, intervals.Count - 1);

            if (type_comparer.Compare(value, this[nearestIndex].End) > 0)
            {
                while (type_comparer.Compare(value, this[nearestIndex].End) > 0 && nearestIndex < intervals.Count - 1)
                    nearestIndex++;
            }
            else
            {
                while (type_comparer.Compare(value, this[nearestIndex].Start) < 0 && nearestIndex > 0)
                    nearestIndex--;
            }

            var nearestInterval = this[nearestIndex];

            return type_comparer.Compare(value, nearestInterval.Start) >= 0 &&
                   type_comparer.Compare(value, nearestInterval.End) <= 0;
        }

        /// <summary>
        /// Adds a new interval to the list.
        /// </summary>
        /// <param name="start">The start value of the interval.</param>
        /// <param name="end">The end value of the interval.</param>
        public void Add(T start, T end) => Add(new Interval<T>(start, end));

        /// <summary>
        /// Adds a new interval to the list
        /// </summary>
        /// <param name="interval">The interval to add.</param>
        public void Add(Interval<T> interval) => intervals.Add(interval);

        /// <summary>
        /// Removes an existing interval from the list.
        /// </summary>
        /// <param name="interval">The interval to remove.</param>
        /// <returns>Whether the provided interval exists in the list and has been removed.</returns>
        public bool Remove(Interval<T> interval) => intervals.Remove(interval);

        /// <summary>
        /// Removes all intervals from the list.
        /// </summary>
        public void Clear() => intervals.Clear();

        public IEnumerator<Interval<T>> GetEnumerator() => intervals.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly struct Interval<T>
        where T : struct, IConvertible
    {
        /// <summary>
        /// The start value of this interval.
        /// </summary>
        public readonly T Start;

        /// <summary>
        /// The end value of this interval.
        /// </summary>
        public readonly T End;

        public Interval(T start, T end)
        {
            if (Comparer<T>.Default.Compare(start, end) >= 0)
                throw new ArgumentException($"Invalid interval, {nameof(start)} must be less than {nameof(end)}", nameof(start));

            Start = start;
            End = end;
        }
    }
}
