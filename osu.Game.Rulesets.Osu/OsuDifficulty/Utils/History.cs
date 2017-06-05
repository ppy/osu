// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.OsuDifficulty.Utils
{
    /// <summary>
    /// An indexed stack with Push() only, which disposes items at the bottom once the size limit has been reached.
    /// Indexing starts at the top of the stack.
    /// </summary>
    public class History<T> : IEnumerable<T>
    {
        public int Count { get; private set; }

        private readonly T[] array;
        private readonly int size;
        private int marker; // Marks the position of the most recently added item.

        public History(int size)
        {
            this.size = size;
            array = new T[size];
            marker = size; // Set marker to the end of the array, outside of the indexed range by one.
        }

        public T this[int i] // Index 0 returns the most recently added item.
        {
            get
            {
                if (i > Count - 1)
                    throw new IndexOutOfRangeException();

                i += marker;
                if (i > size - 1)
                    i -= size;

                return array[i];
            }
        }

        /// <summary>
        /// Adds the element as the most recent one in the history.
        /// The oldest element is disposed if the history is full.
        /// </summary>
        public void Push(T item) // Overwrite the oldest item instead of shifting every item by one with every addition.
        {
            if (marker == 0)
                marker = size - 1;
            else
                --marker;

            array[marker] = item;

            if (Count < size)
                ++Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = marker; i < size; ++i)
                yield return array[i];

            if (Count == size)
                for (int i = 0; i < marker; ++i)
                    yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
