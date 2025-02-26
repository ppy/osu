// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public static class IntervalGroupingUtils
    {
        public static List<List<T>> GroupByInterval<T>(IReadOnlyList<T> objects) where T : IHasInterval
        {
            var groups = new List<List<T>>();

            int i = 0;
            while (i < objects.Count)
                groups.Add(createNextGroup(objects, ref i));

            return groups;
        }

        private static List<T> createNextGroup<T>(IReadOnlyList<T> objects, ref int i) where T : IHasInterval
        {
            const double margin_of_error = 5;

            // This never compares the first two elements in the group.
            // This sounds wrong but is apparently "as intended" (https://github.com/ppy/osu/pull/31636#discussion_r1942673329)
            var groupedObjects = new List<T> { objects[i] };
            i++;

            for (; i < objects.Count - 1; i++)
            {
                if (!Precision.AlmostEquals(objects[i].Interval, objects[i + 1].Interval, margin_of_error))
                {
                    // When an interval change occurs, include the object with the differing interval in the case it increased
                    // See https://github.com/ppy/osu/pull/31636#discussion_r1942368372 for rationale.
                    if (objects[i + 1].Interval > objects[i].Interval + margin_of_error)
                    {
                        groupedObjects.Add(objects[i]);
                        i++;
                    }

                    return groupedObjects;
                }

                // No interval change occurred
                groupedObjects.Add(objects[i]);
            }

            // Check if the last two objects in the object form a "flat" rhythm pattern within the specified margin of error.
            // If true, add the current object to the group and increment the index to process the next object.
            if (objects.Count > 2 && i < objects.Count && Precision.AlmostEquals(objects[^1].Interval, objects[^2].Interval, margin_of_error))
            {
                groupedObjects.Add(objects[i]);
                i++;
            }

            return groupedObjects;
        }
    }
}
