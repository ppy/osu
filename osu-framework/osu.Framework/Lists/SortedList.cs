//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace osu.Framework.Lists
{
    class SortedList<T> : ReadOnlyList<T>, IEnumerable<T>
    {
        private IComparer<T> comparer;

        public SortedList(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        public int Capacity => InternalList.Capacity;
        public int Count => InternalList.Count;
        public bool IsFixedSize => ((IList)InternalList).IsFixedSize;
        public bool IsReadOnly => ((IList)InternalList).IsReadOnly;
        public bool IsSynchronized => ((IList)InternalList).IsSynchronized;
        public object SyncRoot => ((IList)InternalList).SyncRoot;

        public virtual int Add(T value)
        {
            Debug.Assert(value != null);
            Debug.Assert(value is T);

            int index = getAdditionIndex((T)value);
            InternalList.Insert(index, (T)value);

            return index;
        }

        /// <summary>
        /// Gets the first index of the element larger than value.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The index of the first element larger than value.</returns>
        private int getAdditionIndex(T value)
        {
            int index = BinarySearch(value, comparer);
            if (index < 0)
                index = ~index;

            // Binary search is not guaranteed to give the last index 
            // when duplicates are involved, so let's move towards it
            for (; index < Count; index++)
            {
                if (comparer.Compare(this[index], value) != 0)
                    break;
            }

            return index;
        }

        public virtual void Clear()
        {
            InternalList.Clear();
        }

        public virtual bool Remove(T item)
        {
            return InternalList.Remove(item);
        }

        public virtual void RemoveAt(int index)
        {
            InternalList.RemoveAt(index);
        }
    }
}
