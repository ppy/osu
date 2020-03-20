// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// Represents a manual and automated combo indices.
    /// </summary>
    public readonly struct ComboIndex : IEquatable<ComboIndex>
    {
        /// <summary>
        /// The automated index value of the combo in relation to the beatmap.
        /// </summary>
        /// <remarks>
        /// This is separated from <see cref="Manual"/> to disallow
        /// manual combo colouring when not on the right skin to apply.
        /// </remarks>
        public readonly int Automated;

        /// <summary>
        /// The manual index value of the combo, this accounts for <see cref="IHasCombo.ComboOffset"/>.
        /// </summary>
        public readonly int Manual;

        public ComboIndex(int automated, int manual)
        {
            Automated = automated;
            Manual = manual;
        }

        public ComboIndex(int index)
            : this(index, index)
        {
        }

        public override string ToString() => $"(Automated: {Automated}, Manual: {Manual})";

        #region Operator overloading

        public static implicit operator ComboIndex(int i) => FromInt32(i);

        public static bool operator ==(ComboIndex left, ComboIndex right) => left.Equals(right);
        public static bool operator !=(ComboIndex left, ComboIndex right) => !(left == right);

        public static ComboIndex operator +(ComboIndex left, ComboIndex right) => Add(left, right);
        public static ComboIndex operator -(ComboIndex left, ComboIndex right) => Subtract(left, right);
        public static ComboIndex operator *(ComboIndex left, ComboIndex right) => Multiply(left, right);
        public static ComboIndex operator /(ComboIndex left, ComboIndex right) => Divide(left, right);

        public static ComboIndex operator ++(ComboIndex ci) => Increment(ci);
        public static ComboIndex operator --(ComboIndex ci) => Decrement(ci);

        public static ComboIndex FromInt32(int i) => new ComboIndex(i);

        public bool Equals(ComboIndex other) => Automated == other.Automated && Manual == other.Manual;
        public override bool Equals(object obj) => obj is ComboIndex other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Automated, Manual);

        public static ComboIndex Add(ComboIndex a, ComboIndex b) => new ComboIndex(a.Automated + b.Automated, a.Manual + b.Manual);
        public static ComboIndex Subtract(ComboIndex a, ComboIndex b) => new ComboIndex(a.Automated - b.Automated, a.Manual - b.Manual);
        public static ComboIndex Multiply(ComboIndex a, ComboIndex b) => new ComboIndex(a.Automated * b.Automated, a.Manual * b.Manual);
        public static ComboIndex Divide(ComboIndex a, ComboIndex b) => new ComboIndex(a.Automated / b.Automated, a.Manual / b.Manual);

        public static ComboIndex Increment(ComboIndex ci) => ci + 1;
        public static ComboIndex Decrement(ComboIndex ci) => ci - 1;

        #endregion
    }
}
