// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A base class for grouping <see cref="IHasInterval"/>s by their interval. In edges where an interval change
    /// occurs, the <see cref="IHasInterval"/> is added to the group with the smaller interval.
    /// </summary>
    public abstract class SameRhythm<ChildType>
        where ChildType : IHasInterval
    {
        public IReadOnlyList<ChildType> Children { get; private set; }

        /// <summary>
        /// Determines if the intervals between two child objects are within a specified margin of error,
        /// indicating that the intervals are effectively "flat" or consistent.
        /// </summary>
        private bool isFlat(ChildType current, ChildType previous, double marginOfError)
        {
            return Math.Abs(current.Interval - previous.Interval) <= marginOfError;
        }

        /// <summary>
        /// Create a new <see cref="SameRhythm{ChildType}"/> from a list of <see cref="IHasInterval"/>s, and add
        /// them to the <see cref="Children"/> list until the end of the group.
        /// </summary>
        /// <param name="data">The list of <see cref="IHasInterval"/>s.</param>
        /// <param name="i">
        /// Index in <paramref name="data"/> to start adding children. This will be modified and should be passed into
        /// the next <see cref="SameRhythm{ChildType}"/>'s constructor.
        /// </param>
        /// <param name="marginOfError">
        /// The margin of error for the interval, within of which no interval change is considered to have occured.
        /// </param>
        protected SameRhythm(List<ChildType> data, ref int i, double marginOfError)
        {
            List<ChildType> children = new List<ChildType>();
            Children = children;
            children.Add(data[i]);
            i++;

            for (; i < data.Count - 1; i++)
            {
                // An interval change occured, add the current data if the next interval is larger.
                if (!isFlat(data[i], data[i + 1], marginOfError))
                {
                    if (data[i + 1].Interval > data[i].Interval + marginOfError)
                    {
                        children.Add(data[i]);
                        i++;
                    }

                    return;
                }

                // No interval change occured
                children.Add(data[i]);
            }

            // Check if the last two objects in the data form a "flat" rhythm pattern within the specified margin of error.
            // If true, add the current object to the group and increment the index to process the next object.
            if (data.Count > 2 && isFlat(data[^1], data[^2], marginOfError))
            {
                children.Add(data[i]);
                i++;
            }
        }
    }
}
