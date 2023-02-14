// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Utils
{
    /// <summary>
    /// An add-only list of doubles that stores equal consecutive values compactly.
    /// </summary>
    public class CompactList : IEnumerable<double>
    {
        /// <summary>
        /// The total number of items in the list.
        /// </summary>
        public int Count { get; private set; }

        private readonly List<Number> items;

        /// <summary>
        /// Constructs a new <see cref="CompactList"/>
        /// </summary>
        public CompactList()
        {
            items = new List<Number>();
            Count = 0;
        }

        /// <summary>
        /// Adds a value to the <see cref="CompactList"/>.
        /// If <paramref name="value"/> equals the current last value, it will be stored compactly.
        /// </summary>
        /// <param name="value">The value to be added to the list.</param>
        /// <param name="count">The amount of times <paramref name="value"/> is added to the list.</param>
        public void Add(double value, int count = 1)
        {
            if (items.LastOrDefault() is Number number && Precision.AlmostEquals(number.Value, value, 1E-10))
                number.Count += count;
            else if (count > 0)
                items.Add(new Number(value, count));

            Count += count;
        }

        /// <summary>
        /// Enumerates the list.
        /// </summary>
        public IEnumerator<double> GetEnumerator()
        {
            foreach (Number number in items)
            {
                for (int i = 0; i < number.Count; i++)
                {
                    yield return number.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class Number
    {
        public double Value { get; private set; }

        public double Count { get; set; }

        public Number(double value, int count)
        {
            Value = value;
            Count = count;
        }
    }
}
