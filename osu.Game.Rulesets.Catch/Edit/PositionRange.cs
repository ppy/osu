// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Catch.Edit
{
    /// <summary>
    /// Represents either the empty range or a closed interval of horizontal positions in the playfield.
    /// A <see cref="PositionRange"/> represents a closed interval if it is <see cref="Min"/> &lt;= <see cref="Max"/>, and represents the empty range otherwise.
    /// </summary>
    public readonly struct PositionRange
    {
        public readonly float Min;
        public readonly float Max;

        public float Length => Math.Max(0, Max - Min);

        public PositionRange(float value)
            : this(value, value)
        {
        }

        public PositionRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public static PositionRange Union(PositionRange a, PositionRange b) => new PositionRange(Math.Min(a.Min, b.Min), Math.Max(a.Max, b.Max));

        /// <summary>
        /// Get the given position flipped (mirrored) for the axis at the center of this range.
        /// Returns the given position unchanged if the range was empty.
        /// </summary>
        public float GetFlippedPosition(float x) => Min <= Max ? Max - (x - Min) : x;

        public static readonly PositionRange EMPTY = new PositionRange(float.PositiveInfinity, float.NegativeInfinity);
    }
}
