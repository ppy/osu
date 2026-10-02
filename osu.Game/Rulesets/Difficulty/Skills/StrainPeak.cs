// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to store the difficulty of a section of a map.
    /// </summary>
    public readonly record struct StrainPeak : IComparable<StrainPeak>
    {
        public StrainPeak(double value, double sectionLength)
        {
            Value = value;
            SectionLength = Math.Round(sectionLength);
        }

        public double Value { get; }
        public double SectionLength { get; }

        // Reverse sort, highest is first.
        public int CompareTo(StrainPeak other) => other.Value.CompareTo(Value);
    }
}
