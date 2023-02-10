// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Utils
{
    /// <summary>
    /// An add-only list of doubles that stores consecutive zeros compactly.
    /// </summary>
    public class CompressedZeroList : IEnumerable<double>
    {
        /// <summary>
        /// The total number of items in the list.
        /// </summary>
        public int Count { get; private set; }

        private List<Number> items;

        /// <summary>
        /// Constructs a new <see cref="CompressedZeroList"/>
        /// </summary>
        public CompressedZeroList()
        {
            items = new List<Number>();
            Count = 0;
        }

        /// <summary>
        /// Adds a value to the <see cref="CompressedZeroList"/>.
        /// If the value is 0 it will be stored compactly.
        /// </summary>
        /// <param name="value">The value to be added to the list.</param>
        public void Add(double value)
        {
            if (Precision.DefinitelyBigger(value, 0))
                items.Add(new NonZero(value));
            else if (items.LastOrDefault() is Zeros zeros)
                zeros.Add();
            else
                items.Add(new Zeros());

            Count += 1;
        }

        /// <summary>
        /// Enumerates the list.
        /// </summary>
        public IEnumerator<double> GetEnumerator()
        {
            foreach (Number item in items)
            {
                switch (item)
                {
                    case NonZero nonZero:
                        yield return nonZero.Value;
                        break;
                    case Zeros zeros:
                        for (int i = 0; i < zeros.Count; i++)
                            yield return 0.0;

                        break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal abstract class Number
    {
    }

    internal class NonZero : Number
    {
        public double Value { get; private set; }

        public NonZero(double value)
        {
            Value = value;
        }
    }

    internal class Zeros : Number
    {
        public int Count { get; private set; }

        public Zeros()
        {
            Count = 1;
        }

        public void Add()
        {
            Count += 1;
        }
    }
}
