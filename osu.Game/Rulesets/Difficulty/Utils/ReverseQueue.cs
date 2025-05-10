// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// An indexed queue where items are indexed beginning from the most recently enqueued item.
    /// Enqueuing an item pushes all existing indexes up by one and inserts the item at index 0.
    /// Dequeuing an item removes the item from the highest index and returns it.
    /// </summary>
    public class ReverseQueue<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of elements in the <see cref="ReverseQueue{T}"/>.
        /// </summary>
        public int Count { get; private set; }

        private T[] items;
        private int capacity;
        private int start;

        public ReverseQueue(int initialCapacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);

            items = new T[initialCapacity];
            capacity = initialCapacity;
            start = 0;
            Count = 0;
        }

        /// <summary>
        /// Retrieves the item at an index in the <see cref="ReverseQueue{T}"/>.
        /// </summary>
        /// <param name="index">The index of the item to retrieve. The most recently enqueued item is at index 0.</param>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index > Count - 1)
                    throw new ArgumentOutOfRangeException(nameof(index));

                int reverseIndex = Count - 1 - index;
                return items[(start + reverseIndex) % capacity];
            }
        }

        /// <summary>
        /// Enqueues an item to this <see cref="ReverseQueue{T}"/>.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            if (Count == capacity)
            {
                // Double the buffer size
                var buffer = new T[capacity * 2];

                // Copy items to new queue
                for (int i = 0; i < Count; i++)
                {
                    buffer[i] = items[(start + i) % capacity];
                }

                // Replace array with new buffer
                items = buffer;
                capacity *= 2;
                start = 0;
            }

            items[(start + Count) % capacity] = item;
            Count++;
        }

        /// <summary>
        /// Dequeues the least recently enqueued item from the <see cref="ReverseQueue{T}"/> and returns it.
        /// </summary>
        /// <returns>The item dequeued from the <see cref="ReverseQueue{T}"/>.</returns>
        public T Dequeue()
        {
            var item = items[start];
            start = (start + 1) % capacity;
            Count--;
            return item;
        }

        /// <summary>
        /// Clears the <see cref="ReverseQueue{T}"/> of all items.
        /// </summary>
        public void Clear()
        {
            start = 0;
            Count = 0;
        }

        /// <summary>
        /// Returns an enumerator which enumerates items in the <see cref="ReverseQueue{T}"/> starting from the most recently enqueued item.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private ReverseQueue<T> reverseQueue;
            private int currentIndex;

            internal Enumerator(ReverseQueue<T> reverseQueue)
            {
                this.reverseQueue = reverseQueue;
                currentIndex = -1; // The first MoveNext() should bring the iterator to 0
            }

            public bool MoveNext() => ++currentIndex < reverseQueue.Count;

            public void Reset() => currentIndex = -1;

            public readonly T Current => reverseQueue[currentIndex];

            readonly object IEnumerator.Current => Current;

            public void Dispose()
            {
                reverseQueue = null;
            }
        }
    }
}
