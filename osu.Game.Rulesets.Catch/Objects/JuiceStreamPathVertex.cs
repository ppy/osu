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
        public readonly double Time;

        public readonly float X;

        public JuiceStreamPathVertex(double time, float x)
        {
            Time = time;
            X = x;
        }

        public int CompareTo(JuiceStreamPathVertex other)
        {
            int c = Time.CompareTo(other.Time);
            return c != 0 ? c : X.CompareTo(other.X);
        }

        public override string ToString() => $"({Time}, {X})";
    }
}
