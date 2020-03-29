// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// Represents a structure providing an ordinal combo index and also one that accounts for <see cref="IHasCombo.ComboOffset"/>.
    /// </summary>
    public readonly struct ComboIndex : IEquatable<ComboIndex>
    {
        /// <summary>
        /// The ordinal index value of the combo in relation to the beatmap. Ignores <see cref="IHasCombo.ComboOffset"/>.
        /// </summary>
        /// <remarks>
        /// This is separated from <see cref="WithBeatmapOffset"/> to
        /// disallow manual combo colouring when not on the beatmap's skin.
        /// </remarks>
        public readonly int Ordinal;

        /// <summary>
        /// The index value of the combo with account for <see cref="IHasCombo.ComboOffset"/>.
        /// </summary>
        public readonly int WithBeatmapOffset;

        /// <summary>
        /// Constructs a new <see cref="ComboIndex"/> with <paramref name="ordinal"/> set to <see cref="Ordinal"/> and <paramref name="withBeatmapOffset"/> set to <see cref="WithBeatmapOffset"/>.
        /// </summary>
        /// <param name="ordinal">The value to be set to <see cref="Ordinal"/>.</param>
        /// <param name="withBeatmapOffset">The value to be set to <see cref="WithBeatmapOffset"/>.</param>
        public ComboIndex(int ordinal, int withBeatmapOffset)
        {
            Ordinal = ordinal;
            WithBeatmapOffset = withBeatmapOffset;
        }

        /// <summary>
        /// Constructs a new <see cref="ComboIndex"/> with <paramref name="index"/> set to both <see cref="Ordinal"/> and <see cref="WithBeatmapOffset"/>.
        /// </summary>
        /// <param name="index">The value to be set to both <see cref="Ordinal"/> and <see cref="WithBeatmapOffset"/>.</param>
        public ComboIndex(int index)
            : this(index, index)
        {
        }

        public override string ToString() => $"(Ordinal: {Ordinal}, WithBeatmapOffset: {WithBeatmapOffset})";

        #region Operator overloading & simple operation methods

        /// <summary>
        /// Converts <paramref name="i"/> to a <see cref="ComboIndex"/> by constructing one
        /// with <paramref name="i"/> set to both <see cref="Ordinal"/> and <see cref="WithBeatmapOffset"/>.
        /// </summary>
        /// <param name="i">The value to convert to.</param>
        /// <returns>The <see cref="ComboIndex"/> resulted from the conversion.</returns>
        public static implicit operator ComboIndex(int i) => FromInt32(i);

        public static bool operator ==(ComboIndex left, ComboIndex right) => left.Equals(right);
        public static bool operator !=(ComboIndex left, ComboIndex right) => !(left == right);

        /// <summary>
        /// Converts <paramref name="index"/> to a <see cref="ComboIndex"/> by constructing one
        /// with <paramref name="index"/> set to both <see cref="Ordinal"/> and <see cref="WithBeatmapOffset"/>.
        /// </summary>
        /// <param name="index">The value to convert to.</param>
        /// <returns>The <see cref="ComboIndex"/> resulted from the conversion.</returns>
        public static ComboIndex FromInt32(int index) => new ComboIndex(index);

        public bool Equals(ComboIndex other) => Ordinal == other.Ordinal && WithBeatmapOffset == other.WithBeatmapOffset;
        public override bool Equals(object obj) => obj is ComboIndex other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Ordinal, WithBeatmapOffset);

        /// <summary>
        /// Adds <paramref name="amount"/> to both <see cref="Ordinal"/> and <see cref="WithBeatmapOffset"/> of <paramref name="ci"/>.
        /// </summary>
        /// <param name="ci">The <see cref="ComboIndex"/> to perform the addition on.</param>
        /// <param name="amount">The amount to add to <paramref name="ci"/>.</param>
        /// <returns>The <see cref="ComboIndex"/> post-operation.</returns>
        public static ComboIndex Add(ComboIndex ci, int amount) => new ComboIndex(ci.Ordinal + amount, ci.WithBeatmapOffset + amount);

        /// <summary>
        /// Subtracts <paramref name="amount"/> from both <see cref="Ordinal"/> and <see cref="WithBeatmapOffset"/> of <paramref name="ci"/>.
        /// </summary>
        /// <param name="ci">The <see cref="ComboIndex"/> to perform the subtraction on.</param>
        /// <param name="amount">The amount to subtract from <paramref name="ci"/>.</param>
        /// <returns>The <see cref="ComboIndex"/> post-operation.</returns>
        public static ComboIndex Subtract(ComboIndex ci, int amount) => new ComboIndex(ci.Ordinal - amount, ci.WithBeatmapOffset - amount);

        #endregion
    }
}
