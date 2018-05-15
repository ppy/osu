// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// An indexed stack with Push() only, which disposes items at the bottom after the capacity is full.
    /// Indexing starts at the top of the stack.
    /// </summary>
    public class History<T> : IEnumerable<T>
    {
        public int Count { get; private set; }

        private readonly T[] array;
        private readonly int capacity;
        private int marker; // Marks the position of the most recently added item.

        /// <summary>
        /// Initializes a new instance of the History class that is empty and has the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of items the History can hold.</param>
        public History(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();

            this.capacity = capacity;
            array = new T[capacity];
            marker = capacity; // Set marker to the end of the array, outside of the indexed range by one.
        }

        /// <summary>
        /// The most recently added item is returned at index 0.
        /// </summary>
        public T this[int i]
        {
            get
            {
                if (i < 0 || i > Count - 1)
                    throw new IndexOutOfRangeException();

                i += marker;
                if (i > capacity - 1)
                    i -= capacity;

                return array[i];
            }
        }

        /// <summary>
        /// Adds the item as the most recent one in the history.
        /// The oldest item is disposed if the history is full.
        /// </summary>
        public void Push(T item) // Overwrite the oldest item instead of shifting every item by one with every addition.
        {
            if (marker == 0)
                marker = capacity - 1;
            else
                --marker;

            array[marker] = item;

            if (Count < capacity)
                ++Count;
        }

        /// <summary>
        /// Returns an enumerator which enumerates items in the history starting from the most recently added one.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = marker; i < capacity; ++i)
                yield return array[i];

            if (Count == capacity)
                for (int i = 0; i < marker; ++i)
                    yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
