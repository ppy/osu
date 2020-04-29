// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Utils
{
    /// <summary>
    /// Represents a tracking component used for whether a
    /// specific time falls into any of the provided periods.
    /// </summary>
    public class PeriodTracker
    {
        private readonly List<Period> periods = new List<Period>();
        private int nearestIndex;

        /// <summary>
        /// The list of periods to add to the tracker for using the required check methods.
        /// </summary>
        public IEnumerable<Period> Periods
        {
            set
            {
                var sortedValue = value?.ToList();
                sortedValue?.Sort();

                if (sortedValue != null && periods.SequenceEqual(sortedValue))
                    return;

                periods.Clear();
                nearestIndex = 0;

                if (value?.Any() != true)
                    return;

                periods.AddRange(sortedValue);
            }
        }

        /// <summary>
        /// Whether the provided time is in any of the added periods.
        /// </summary>
        /// <param name="time">The time value to check for.</param>
        public bool Contains(double time)
        {
            if (periods.Count == 0)
                return false;

            if (time > periods[nearestIndex].End)
            {
                while (time > periods[nearestIndex].End && nearestIndex < periods.Count - 1)
                    nearestIndex++;
            }
            else
            {
                while (time < periods[nearestIndex].Start && nearestIndex > 0)
                    nearestIndex--;
            }

            var nearest = periods[nearestIndex];
            return time >= nearest.Start && time <= nearest.End;
        }
    }

    public readonly struct Period : IComparable<Period>
    {
        /// <summary>
        /// The start time of this period.
        /// </summary>
        public readonly double Start;

        /// <summary>
        /// The end time of this period.
        /// </summary>
        public readonly double End;

        public Period(double start, double end)
        {
            if (start >= end)
                throw new ArgumentException($"Invalid period provided, {nameof(start)} must be less than {nameof(end)}", nameof(start));

            Start = start;
            End = end;
        }

        public int CompareTo(Period other) => Start.CompareTo(other.Start);
    }
}
