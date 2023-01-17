// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace osu.Game.Rulesets.Mania.MathUtils
{
    /// <summary>
    /// Provides access to .NET4.0 unstable sorting methods.
    /// </summary>
    /// <remarks>
    /// Source: https://referencesource.microsoft.com/#mscorlib/system/collections/generic/arraysorthelper.cs
    /// Copyright (c) Microsoft Corporation.  All rights reserved.
    /// </remarks>
    internal static class LegacySortHelper<T>
    {
        private const int quick_sort_depth_threshold = 32;

        public static void Sort(T[] keys, IComparer<T> comparer)
        {
            ArgumentNullException.ThrowIfNull(keys);

            if (keys.Length == 0)
                return;

            comparer ??= Comparer<T>.Default;
            depthLimitedQuickSort(keys, 0, keys.Length - 1, comparer, quick_sort_depth_threshold);
        }

        private static void depthLimitedQuickSort(T[] keys, int left, int right, IComparer<T> comparer, int depthLimit)
        {
            do
            {
                if (depthLimit == 0)
                {
                    heapsort(keys, left, right, comparer);
                    return;
                }

                int i = left;
                int j = right;

                // pre-sort the low, middle (pivot), and high values in place.
                // this improves performance in the face of already sorted data, or
                // data that is made up of multiple sorted runs appended together.
                int middle = i + ((j - i) >> 1);
                swapIfGreater(keys, comparer, i, middle); // swap the low with the mid point
                swapIfGreater(keys, comparer, i, j); // swap the low with the high
                swapIfGreater(keys, comparer, middle, j); // swap the middle with the high

                T x = keys[middle];

                do
                {
                    while (comparer.Compare(keys[i], x) < 0) i++;
                    while (comparer.Compare(x, keys[j]) < 0) j--;
                    Contract.Assert(i >= left && j <= right, "(i>=left && j<=right)  Sort failed - Is your IComparer bogus?");
                    if (i > j) break;

                    if (i < j)
                    {
                        (keys[i], keys[j]) = (keys[j], keys[i]);
                    }

                    i++;
                    j--;
                } while (i <= j);

                // The next iteration of the while loop is to "recursively" sort the larger half of the array and the
                // following calls recrusively sort the smaller half.  So we subtrack one from depthLimit here so
                // both sorts see the new value.
                depthLimit--;

                if (j - left <= right - i)
                {
                    if (left < j) depthLimitedQuickSort(keys, left, j, comparer, depthLimit);
                    left = i;
                }
                else
                {
                    if (i < right) depthLimitedQuickSort(keys, i, right, comparer, depthLimit);
                    right = j;
                }
            } while (left < right);
        }

        private static void heapsort(T[] keys, int lo, int hi, IComparer<T> comparer)
        {
            Contract.Requires(keys != null);
            Contract.Requires(comparer != null);
            Contract.Requires(lo >= 0);
            Contract.Requires(hi > lo);
            Contract.Requires(hi < keys.Length);

            int n = hi - lo + 1;

            for (int i = n / 2; i >= 1; i = i - 1)
            {
                downHeap(keys, i, n, lo, comparer);
            }

            for (int i = n; i > 1; i = i - 1)
            {
                swap(keys, lo, lo + i - 1);
                downHeap(keys, 1, i - 1, lo, comparer);
            }
        }

        private static void downHeap(T[] keys, int i, int n, int lo, IComparer<T> comparer)
        {
            Contract.Requires(keys != null);
            Contract.Requires(comparer != null);
            Contract.Requires(lo >= 0);
            Contract.Requires(lo < keys.Length);

            T d = keys[lo + i - 1];

            while (i <= n / 2)
            {
                int child = 2 * i;

                if (child < n && comparer.Compare(keys[lo + child - 1], keys[lo + child]) < 0)
                {
                    child++;
                }

                if (!(comparer.Compare(d, keys[lo + child - 1]) < 0))
                    break;

                keys[lo + i - 1] = keys[lo + child - 1];
                i = child;
            }

            keys[lo + i - 1] = d;
        }

        private static void swap(T[] a, int i, int j)
        {
            if (i != j)
                (a[i], a[j]) = (a[j], a[i]);
        }

        private static void swapIfGreater(T[] keys, IComparer<T> comparer, int a, int b)
        {
            if (a != b)
            {
                if (comparer.Compare(keys[a], keys[b]) > 0)
                    (keys[a], keys[b]) = (keys[b], keys[a]);
            }
        }
    }
}
