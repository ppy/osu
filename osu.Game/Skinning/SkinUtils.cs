// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Contains helper methods to assist in implementing <see cref="ISkin"/>s.
    /// </summary>
    public static class SkinUtils
    {
        /// <summary>
        /// Converts an <see cref="object"/> to a <see cref="Bindable{TValue}"/>. Used for returning configuration values of specific types.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <typeparam name="TValue">The type of value <paramref name="value"/>, and the type of the resulting bindable.</typeparam>
        /// <returns>The resulting bindable.</returns>
        public static Bindable<TValue>? As<TValue>(object? value) => (Bindable<TValue>?)value;
    }
}
