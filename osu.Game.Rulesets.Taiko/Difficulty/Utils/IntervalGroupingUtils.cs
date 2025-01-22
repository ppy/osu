// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public static class IntervalGroupingUtils
    {
        public static List<List<T>> GroupByInterval<T>(IReadOnlyList<T> data, double marginOfError = 5) where T : IHasInterval
        {
            var groups = new List<List<T>>();
            if (data.Count == 0)
                return groups;

            int i = 0;

            while (i < data.Count)
            {
                var group = createGroup(data, ref i, marginOfError);
                groups.Add(group);
            }

            return groups;
        }

        private static List<T> createGroup<T>(IReadOnlyList<T> data, ref int i, double marginOfError) where T : IHasInterval
        {
            var children = new List<T> { data[i] };
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

                    return children;
                }

                // No interval change occurred
                children.Add(data[i]);
            }

            // Check if the last two objects in the data form a "flat" rhythm pattern within the specified margin of error.
            // If true, add the current object to the group and increment the index to process the next object.
            if (data.Count > 2 && i < data.Count &&
                Precision.AlmostEquals(data[^1].Interval, data[^2].Interval, marginOfError))
            {
                children.Add(data[i]);
                i++;
            }

            return children;
        }
    }
}
