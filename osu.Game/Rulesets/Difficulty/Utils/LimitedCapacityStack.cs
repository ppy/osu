// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// An indexed stack with limited depth. Indexing starts at the top of the stack.
    /// </summary>
    public class LimitedCapacityStack<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of elements in the stack.
        /// </summary>
        public int Count { get; private set; }

        private readonly T[] array;
        private readonly int capacity;
        private int marker; // Marks the position of the most recently added item.

        /// <summary>
        /// Constructs a new <see cref="LimitedCapacityStack{T}"/>.
        /// </summary>
        /// <param name="capacity">The number of items the stack can hold.</param>
        public LimitedCapacityStack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();

            this.capacity = capacity;
            array = new T[capacity];
            marker = capacity; // Set marker to the end of the array, outside of the indexed range by one.
        }

        /// <summary>
        /// Retrieves the item at an index in the stack.
        /// </summary>
        /// <param name="i">The index of the item to retrieve. The top of the stack is returned at index 0.</param>
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
        /// Pushes an item to this <see cref="LimitedCapacityStack{T}"/>.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            // Overwrite the oldest item instead of shifting every item by one with every addition.
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
