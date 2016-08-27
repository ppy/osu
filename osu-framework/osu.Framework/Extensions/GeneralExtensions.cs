//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

// this is an abusive thing to do, but it increases the visibility of Extension Methods to virtually every file.
namespace System
{
    /// <summary>
    /// This class holds extension methods for various purposes and should not be used explicitly, ever.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The predicate that needs to be matched.</param>
        /// <param name="startIndex">The index to start conditional search.</param>
        /// <returns>The matched item, or the default value for the type if no item was matched.</returns>
        public static T Find<T>(this List<T> list, Predicate<T> match, int startIndex)
        {
            if (!list.IsValidIndex(startIndex)) return default(T);

            int val = list.FindIndex(startIndex, list.Count - startIndex - 1, match);

            return list.ValueAtOrDefault(val);
        }

        /// <summary>
        /// Adds the given item to the list according to standard sorting rules. Do not use on unsorted lists.
        /// </summary>
        /// <param name="item">The item that should be added.</param>
        /// <returns>The index in the list where the item was inserted.</returns>
        public static int AddInPlace<T>(this List<T> list, T item)
        {
            int index = list.BinarySearch(item);
            if (index < 0) index = ~index; // BinarySearch hacks multiple return values with 2's complement.
            list.Insert(index, item);
            return index;
        }
        /// <summary>
        /// Adds the given item to the list according to the comparers sorting rules. Do not use on unsorted lists.
        /// </summary>
        /// <param name="item">The item that should be added.</param>
        /// <param name="comparer">The comparer that should be used for sorting.</param>
        /// <returns>The index in the list where the item was inserted.</returns>
        public static int AddInPlace<T>(this List<T> list, T item, IComparer<T> comparer)
        {
            int index = list.BinarySearch(item, comparer);
            if (index < 0) index = ~index; // BinarySearch hacks multiple return values with 2's complement.
            list.Insert(index, item);
            return index;
        }

        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        /// <summary>
        /// Validates whether index is valid, before returning the value at the given index.
        /// </summary>
        /// <typeparam name="T">Probably should limit to nullable types.</typeparam>
        /// <param name="list">The list to take values</param>
        /// <param name="index">The index to request values from</param>
        /// <returns>Value at index, else the default value</returns>
        public static T ValueAtOrDefault<T>(this List<T> list, int index)
        {
            return list.IsValidIndex(index) ? list[index] : default(T);
        }

        /// <summary>
        /// Compares every item in list to given list.
        /// </summary>
        public static bool CompareTo<T>(this List<T> list, List<T> list2)
        {
            if (list.Count != list2.Count) return false;

            return !list.Where((t, i) => !t.Equals(list2[i])).Any();
        }

        public static string ToResolutionString(this System.Drawing.Size size)
        {
            return size.Width.ToString() + 'x' + size.Height.ToString();
        }

        public static void WriteLineExplicit(this Stream s, string str = @"")
        {
            byte[] data = Encoding.UTF8.GetBytes($"{str}\r\n");
            s.Write(data, 0, data.Length);
        }

        public static string UnsecureRepresentation(this SecureString s)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(s);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        public static long ToUnixTimestamp(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
