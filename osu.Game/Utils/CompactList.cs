// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using osu.Framework.Utils;

namespace osu.Game.Utils
{
    /// <summary>
    /// An add-only list of doubles that stores equal consecutive values compactly.
    /// </summary>
    public class CompactList : IEnumerable<double>
    {
        // As this type is used for storing strains during difficulty calculation,
        // distinguishing even the smallest values properly is crucial to ensure the same
        // results as using a simple List which was used prior to this optimization.
        // Even machine epsilon (2.22E-16) has been observed to cause inaccuracies (/b/100843 + EZ)
        // so the acceptable difference has to be even smaller to discern one double from another.
        public const double ACCEPTABLE_DIFFERENCE = 1E-16;

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
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (items.Count > 0 && Precision.AlmostEquals(items[^1].Value, value, ACCEPTABLE_DIFFERENCE))
                items[^1] = new Number(value, items[^1].Count + count);
            else
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

        private readonly struct Number
        {
            public readonly double Value;

            public readonly int Count;

            public Number(double value, int count)
            {
                Value = value;
                Count = count;
            }
        }
    }
}
