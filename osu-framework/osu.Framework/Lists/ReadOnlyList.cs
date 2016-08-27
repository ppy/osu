//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Lists
{
    /// <summary>
    /// A simple list that implements all List extensions but does not
    /// have methods to change the list after initial construction.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    public class ReadOnlyList<T> : IReadOnlyList<T>
    {
        protected List<T> InternalList = new List<T>();

        /// <summary>
        /// Constructs a ReadOnlyList.
        /// </summary>
        public ReadOnlyList() { }

        /// <summary>
        /// Constructs a ReadOnlyList which wraps over an existing List.
        /// </summary>
        /// <param name="list">The list to wrap over.</param>
        public ReadOnlyList(List<T> list)
        {
            this.InternalList.AddRange(list);
        }

        /// <summary>
        /// Constructs a ReadOnlyList from a collection of items.
        /// </summary>
        /// <param name="collection">The collection to add to the list.</param>
        public ReadOnlyList(IEnumerable<T> collection)
        {
            this.InternalList.AddRange(collection);
        }

        public T this[int index] => InternalList[index];
        public int Capacity => InternalList.Capacity;
        public int Count => InternalList.Count;

        public int BinarySearch(T item)
        {
            return InternalList.BinarySearch(item);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return InternalList.BinarySearch(item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return InternalList.BinarySearch(index, count, item, comparer);
        }

        public bool Contains(T item)
        {
            return InternalList.Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return InternalList.ConvertAll(converter);
        }

        public void CopyTo(T[] array)
        {
            InternalList.CopyTo(array);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InternalList.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            InternalList.CopyTo(index, array, arrayIndex, count);
        }

        public bool Exists(Predicate<T> match)
        {
            return InternalList.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return InternalList.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return InternalList.FindAll(match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return InternalList.FindIndex(match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return InternalList.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return InternalList.FindIndex(startIndex, count, match);
        }

        public T FindLast(Predicate<T> match)
        {
            return InternalList.FindLast(match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return InternalList.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return InternalList.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return InternalList.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            InternalList.ForEach(action);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        public List<T> GetRange(int index, int count)
        {
            return InternalList.GetRange(index, count);
        }

        public int IndexOf(T item)
        {
            return InternalList.IndexOf(item);
        }

        public int IndexOf(T item, int index)
        {
            return InternalList.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return InternalList.IndexOf(item, index, count);
        }

        public int LastIndexOf(T item)
        {
            return InternalList.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index)
        {
            return InternalList.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return InternalList.LastIndexOf(item, index, count);
        }

        public bool TrueForAll(Predicate<T> match)
        {
            return InternalList.TrueForAll(match);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        public IEnumerable Reverse()
        {
            for (int i = InternalList.Count - 1; i >= 0; i--)
                yield return InternalList[i];
        }
    }
}
