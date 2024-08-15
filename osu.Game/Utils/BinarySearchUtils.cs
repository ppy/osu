// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Utils
{
    public class BinarySearchUtils
    {
        /// <summary>
        /// Finds the index of the item in the sorted list which has its property equal to the search term.
        /// If no exact match is found, the complement of the index of the first item greater than the search term will be returned.
        /// </summary>
        /// <typeparam name="T">The type of the items in the list to search.</typeparam>
        /// <typeparam name="T2">The type of the property to perform the search on.</typeparam>
        /// <param name="list">The list of items to search.</param>
        /// <param name="searchTerm">The query to find.</param>
        /// <param name="termFunc">Function that maps an item in the list to its index property.</param>
        /// <param name="equalitySelection">Determines which index to return if there are multiple exact matches.</param>
        /// <returns>The index of the found item. Will return the complement of the index of the first item greater than the search query if no exact match is found.</returns>
        public static int BinarySearch<T, T2>(IReadOnlyList<T> list, T2 searchTerm, Func<T, T2> termFunc, EqualitySelection equalitySelection = EqualitySelection.FirstFound)
        {
            int n = list.Count;

            if (n == 0)
                return -1;

            var comparer = Comparer<T2>.Default;

            if (comparer.Compare(searchTerm, termFunc(list[0])) == -1)
                return -1;

            if (comparer.Compare(searchTerm, termFunc(list[^1])) == 1)
                return ~n;

            int min = 0;
            int max = n - 1;
            bool equalityFound = false;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                T2 midTerm = termFunc(list[mid]);

                switch (comparer.Compare(midTerm, searchTerm))
                {
                    case 0:
                        equalityFound = true;

                        switch (equalitySelection)
                        {
                            case EqualitySelection.Leftmost:
                                max = mid - 1;
                                break;

                            case EqualitySelection.Rightmost:
                                min = mid + 1;
                                break;

                            default:
                            case EqualitySelection.FirstFound:
                                return mid;
                        }

                        break;

                    case 1:
                        max = mid - 1;
                        break;

                    case -1:
                        min = mid + 1;
                        break;
                }
            }

            if (!equalityFound) return ~min;

            switch (equalitySelection)
            {
                case EqualitySelection.Leftmost:
                    return min;

                default:
                case EqualitySelection.Rightmost:
                    return min - 1;
            }
        }
    }

    public enum EqualitySelection
    {
        FirstFound,
        Leftmost,
        Rightmost
    }
}
