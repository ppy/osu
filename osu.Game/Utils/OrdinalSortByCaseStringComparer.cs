// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Utils
{
    /// <summary>
    /// This string comparer is something of a cross-over between <see cref="StringComparer.Ordinal"/> and <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// <see cref="StringComparer.OrdinalIgnoreCase"/> is used first, but <see cref="StringComparer.Ordinal"/> is used as a tie-breaker.
    /// </summary>
    /// <remarks>
    /// This comparer's behaviour somewhat emulates <see cref="StringComparer.InvariantCulture"/>,
    /// but non-ordinal comparers - both culture-aware and culture-invariant - have huge performance overheads due to i18n factors (up to 5x slower).
    /// </remarks>
    /// <example>
    /// Given the following strings to sort: <c>[A, B, C, D, a, b, c, d, A]</c> and a stable sorting algorithm:
    /// <list type="bullet">
    /// <item>
    /// <see cref="StringComparer.Ordinal"/> would return <c>[A, A, B, C, D, a, b, c, d]</c>.
    /// This is undesirable as letters are interleaved.
    /// </item>
    /// <item>
    /// <see cref="StringComparer.OrdinalIgnoreCase"/> would return <c>[A, a, A, B, b, C, c, D, d]</c>.
    /// Different letters are not interleaved, but because case is ignored, the As are left in arbitrary order.
    /// </item>
    /// </list>
    /// <item>
    /// <see cref="OrdinalSortByCaseStringComparer"/> would return <c>[a, A, A, b, B, c, C, d, D]</c>, which is the expected behaviour.
    /// </item>
    /// </example>
    public class OrdinalSortByCaseStringComparer : IComparer<string>
    {
        public static readonly OrdinalSortByCaseStringComparer DEFAULT = new OrdinalSortByCaseStringComparer();

        private OrdinalSortByCaseStringComparer()
        {
        }

        public int Compare(string? a, string? b)
        {
            int result = StringComparer.OrdinalIgnoreCase.Compare(a, b);
            if (result == 0)
                result = -StringComparer.Ordinal.Compare(a, b); // negative to place lowercase letters before uppercase.
            return result;
        }
    }
}
