// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Utils
{
    /// <summary>
    /// A wrapper over a value and a boolean denoting whether the value is valid.
    /// </summary>
    /// <typeparam name="T">The type of value stored.</typeparam>
    public readonly ref struct Optional<T>
    {
        /// <summary>
        /// The stored value.
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Whether <see cref="Value"/> is valid.
        /// </summary>
        /// <remarks>
        /// If <typeparamref name="T"/> is a reference type, <c>null</c> may be valid for <see cref="Value"/>.
        /// </remarks>
        public readonly bool HasValue;

        private Optional(T value)
        {
            Value = value;
            HasValue = true;
        }

        /// <summary>
        /// Returns <see cref="Value"/> if it's valid, or a given fallback value otherwise.
        /// </summary>
        /// <remarks>
        /// Shortcase for: <c>optional.HasValue ? optional.Value : fallback</c>.
        /// </remarks>
        /// <param name="fallback">The fallback value to return if <see cref="HasValue"/> is <c>false</c>.</param>
        /// <returns></returns>
        public T GetOr(T fallback) => HasValue ? Value : fallback;

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);
    }
}
