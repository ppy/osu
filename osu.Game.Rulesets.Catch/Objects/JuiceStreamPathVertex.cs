// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

namespace osu.Game.Rulesets.Catch.Objects
{
    /// <summary>
    /// A vertex of a <see cref="JuiceStreamPath"/>.
    /// </summary>
    public readonly struct JuiceStreamPathVertex : IComparable<JuiceStreamPathVertex>
    {
        public readonly double Distance;

        public readonly float X;

        public JuiceStreamPathVertex(double distance, float x)
        {
            Distance = distance;
            X = x;
        }

        public int CompareTo(JuiceStreamPathVertex other)
        {
            int c = Distance.CompareTo(other.Distance);
            return c != 0 ? c : X.CompareTo(other.X);
        }

        public override string ToString() => $"({Distance}, {X})";
    }
}
