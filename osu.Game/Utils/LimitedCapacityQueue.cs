// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Utils
{
    /// <summary>
    /// An indexed queue with limited capacity.
    /// Respects first-in-first-out insertion order.
    /// </summary>
    public class LimitedCapacityQueue<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of elements in the queue.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Whether the queue is full (adding any new items will cause removing existing ones).
        /// </summary>
        public bool Full => Count == capacity;

        private readonly T[] array;
        private readonly int capacity;

        // Markers tracking the queue's first and last element.
        private int start, end;

        /// <summary>
        /// Constructs a new <see cref="LimitedCapacityQueue{T}"/>
        /// </summary>
        /// <param name="capacity">The number of items the queue can hold.</param>
        public LimitedCapacityQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            this.capacity = capacity;
            array = new T[capacity];
            Clear();
        }

        /// <summary>
        /// Removes all elements from the <see cref="LimitedCapacityQueue{T}"/>.
        /// </summary>
        public void Clear()
        {
            start = 0;
            end = -1;
            Count = 0;
        }

        /// <summary>
        /// Removes an item from the front of the <see cref="LimitedCapacityQueue{T}"/>.
        /// </summary>
        /// <returns>The item removed from the front of the queue.</returns>
        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("Queue is empty.");

            var result = array[start];
            start = (start + 1) % capacity;
            Count--;
            return result;
        }

        /// <summary>
        /// Adds an item to the back of the <see cref="LimitedCapacityQueue{T}"/>.
        /// If the queue is holding <see cref="Count"/> elements at the point of addition,
        /// the item at the front of the queue will be removed.
        /// </summary>
        /// <param name="item">The item to be added to the back of the queue.</param>
        public void Enqueue(T item)
        {
            end = (end + 1) % capacity;
            if (Count == capacity)
                start = (start + 1) % capacity;
            else
                Count++;
            array[end] = item;
        }

        /// <summary>
        /// Retrieves the item at the given index in the queue.
        /// </summary>
        /// <param name="index">
        /// The index of the item to retrieve.
        /// The item with index 0 is at the front of the queue
        /// (it was added the earliest).
        /// </param>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return array[(start + index) % capacity];
            }
        }

        /// <summary>
        /// Enumerates the queue from its start to its end.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            if (Count == 0)
                yield break;

            for (int i = 0; i < Count; i++)
                yield return array[(start + i) % capacity];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
