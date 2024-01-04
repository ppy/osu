// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Utils
{
    /// <summary>
    /// Represents a tracking component used for whether a specific time instant falls into any of the provided periods.
    /// </summary>
    public class PeriodTracker
    {
        private readonly List<Period> periods;
        private int nearestIndex;

        public PeriodTracker(IEnumerable<Period> periods)
        {
            this.periods = periods.OrderBy(period => period.Start).ToList();
        }

        /// <summary>
        /// Whether the provided time is in any of the added periods.
        /// </summary>
        /// <param name="time">The time value to check.</param>
        public bool IsInAny(double time)
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

        public Period? GetPeriodIfAny(double time)
        {
            if (IsInAny(time))
                return periods[nearestIndex];

            return null;
        }
    }

    public readonly struct Period
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
                throw new ArgumentException($"Invalid period provided, {nameof(start)} must be less than {nameof(end)}");

            Start = start;
            End = end;
        }
    }
}
