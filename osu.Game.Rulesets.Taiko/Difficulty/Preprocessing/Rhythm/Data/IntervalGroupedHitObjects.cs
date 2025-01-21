// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A base class for grouping <see cref="IHasInterval"/>s by their interval. In edges where an interval change
    /// occurs, the <see cref="IHasInterval"/> is added to the group with the smaller interval.
    /// </summary>
    public abstract class IntervalGroupedHitObjects<TChildType>
        where TChildType : IHasInterval
    {
        public IReadOnlyList<TChildType> Children { get; private set; }

        /// <summary>
        /// Create a new <see cref="IntervalGroupedHitObjects{TChildType}"/> from a list of <see cref="IHasInterval"/>s, and add
        /// them to the <see cref="Children"/> list until the end of the group.
        /// </summary>
        /// <param name="data">The list of <see cref="IHasInterval"/>s.</param>
        /// <param name="i">
        /// Index in <paramref name="data"/> to start adding children. This will be modified and should be passed into
        /// the next <see cref="IntervalGroupedHitObjects{TChildType}"/>'s constructor.
        /// </param>
        /// <param name="marginOfError">
        /// The margin of error for the interval, within of which no interval change is considered to have occured.
        /// </param>
        protected IntervalGroupedHitObjects(List<TChildType> data, ref int i, double marginOfError)
        {
            List<TChildType> children = new List<TChildType>();
            Children = children;
            children.Add(data[i]);
            i++;

            for (; i < data.Count - 1; i++)
            {
                // An interval change occured, add the current data if the next interval is larger.
                if (!Precision.AlmostEquals(data[i].Interval, data[i + 1].Interval, marginOfError))
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
            if (data.Count > 2 && Precision.AlmostEquals(data[^1].Interval, data[^2].Interval, marginOfError))
            {
                children.Add(data[i]);
                i++;
            }
        }
    }
}
