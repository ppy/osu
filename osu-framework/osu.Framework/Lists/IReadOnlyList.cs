//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Lists
{
    /// <summary>
    /// An implementation of System.Collections.Generic.IReadOnlyList
    /// with List&le;T&ge; methods.
    /// </summary>
    interface IReadOnlyList<T> : System.Collections.Generic.IReadOnlyList<T>
    {
        int Capacity { get; }

        int BinarySearch(T item);
        int BinarySearch(T item, IComparer<T> comparer);
        int BinarySearch(int index, int count, T item, IComparer<T> comparer);

        bool Contains(T item);

        List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter);

        void CopyTo(T[] array);
        void CopyTo(T[] array, int arrayIndex);
        void CopyTo(int index, T[] array, int arrayIndex, int count);

        bool Exists(Predicate<T> match);

        T Find(Predicate<T> match);
        List<T> FindAll(Predicate<T> match);

        int FindIndex(int startIndex, int count, Predicate<T> match);
        int FindIndex(int startIndex, Predicate<T> match);
        int FindIndex(Predicate<T> match);

        T FindLast(Predicate<T> match);

        int FindLastIndex(int startIndex, int count, Predicate<T> match);
        int FindLastIndex(int startIndex, Predicate<T> match);
        int FindLastIndex(Predicate<T> match);

        void ForEach(Action<T> action);

        List<T> GetRange(int index, int count);

        int IndexOf(T item);
        int IndexOf(T item, int index);
        int IndexOf(T item, int index, int count);

        int LastIndexOf(T item);
        int LastIndexOf(T item, int index);
        int LastIndexOf(T item, int index, int count);

        bool TrueForAll(Predicate<T> match);
    }
}
