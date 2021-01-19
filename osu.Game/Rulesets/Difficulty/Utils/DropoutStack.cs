// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// An indexed stack where items are dropped from the bottom rather than popped from the top.
    /// Indexing starts at the top of the <see cref="DropoutStack{T}"/>.
    /// </summary>
    public class DropoutStack<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of elements in the <see cref="DropoutStack{T}"/>.
        /// </summary>
        public int Count => items.Count;

        private readonly List<T> items = new List<T>();

        /// <summary>
        /// Retrieves the item at an index in the <see cref="DropoutStack{T}"/>.
        /// </summary>
        /// <param name="i">The index of the item to retrieve. The top of the <see cref="DropoutStack{T}"/> is returned at index 0.</param>
        public T this[int i]
        {
            get
            {
                int reverseIndex = Count - 1 - i;
                return items[reverseIndex];
            }
        }

        /// <summary>
        /// Pushes an item to this <see cref="DropoutStack{T}"/>.
        /// </summary>
        /// <param name="item">The item to push.</param>
        public void Push(T item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Drops the bottom item from the <see cref="DropoutStack{T}"/> and returns it.
        /// </summary>
        /// <returns>The item dropped from the <see cref="DropoutStack{T}"/>.</returns>
        public T Drop()
        {
            var item = items[0];
            items.RemoveAt(0);
            return item;
        }

        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Returns an enumerator which enumerates items in the <see cref="DropoutStack{T}"/> starting from the most recently added one.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                yield return items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
