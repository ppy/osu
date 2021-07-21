// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

namespace osu.Game.Rulesets.Catch.Edit
{
    /// <summary>
    /// Represents a closed interval of horizontal positions in the playfield.
    /// </summary>
    public readonly struct PositionRange
    {
        public readonly float Min;
        public readonly float Max;

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

        public float GetFlippedPosition(float x) => Max - (x - Min);

        public static readonly PositionRange EMPTY = new PositionRange(float.PositiveInfinity, float.NegativeInfinity);
    }
}
