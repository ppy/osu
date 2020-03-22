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
        /// The ordinal index value of the combo in relation to the beatmap. Ignores <see cref="IHasCombo.ComboOffset"/>.
        /// </summary>
        /// <remarks>
        /// This is separated from <see cref="WithOffset"/> to disallow
        /// manual combo colouring when not on the right skin to apply.
        /// </remarks>
        public readonly int Ordinal;

        /// <summary>
        /// The manual index value of the combo. Accounts for <see cref="IHasCombo.ComboOffset"/>.
        /// </summary>
        public readonly int WithOffset;

        /// <summary>
        /// Constructs a new <see cref="ComboIndex"/> with <paramref name="ordinal"/> set to <see cref="Ordinal"/> and <paramref name="withOffset"/> set to <see cref="WithOffset"/>.
        /// </summary>
        /// <param name="ordinal">The value to be set to <see cref="Ordinal"/>.</param>
        /// <param name="withOffset">The value to be set to <see cref="WithOffset"/>.</param>
        public ComboIndex(int ordinal, int withOffset)
        {
            Ordinal = ordinal;
            WithOffset = withOffset;
        }

        /// <summary>
        /// Constructs a new <see cref="ComboIndex"/> with <paramref name="index"/> set to both <see cref="Ordinal"/> and <see cref="WithOffset"/>.
        /// </summary>
        /// <param name="index">The value to be set to both <see cref="Ordinal"/> and <see cref="WithOffset"/>.</param>
        public ComboIndex(int index)
            : this(index, index)
        {
        }

        public override string ToString() => $"(Ordinal: {Ordinal}, WithOffset: {WithOffset})";

        #region Operator overloading

        public static implicit operator ComboIndex(int i) => FromInt32(i);

        public static bool operator ==(ComboIndex left, ComboIndex right) => left.Equals(right);
        public static bool operator !=(ComboIndex left, ComboIndex right) => !(left == right);

        public static ComboIndex FromInt32(int i) => new ComboIndex(i);

        public bool Equals(ComboIndex other) => Ordinal == other.Ordinal && WithOffset == other.WithOffset;
        public override bool Equals(object obj) => obj is ComboIndex other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Ordinal, WithOffset);

        public static ComboIndex Add(ComboIndex ci, int amount) => new ComboIndex(ci.Ordinal + amount, ci.WithOffset + amount);

        public static ComboIndex Subtract(ComboIndex ci, int amount) => new ComboIndex(ci.Ordinal - amount, ci.WithOffset - amount);

        #endregion
    }
}
