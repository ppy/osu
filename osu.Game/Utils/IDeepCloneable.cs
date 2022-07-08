// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    /// <summary>A generic interface for a deeply cloneable type.</summary>
    /// <typeparam name="T">The type of object to clone.</typeparam>
    public interface IDeepCloneable<out T> where T : class
    {
        /// <summary>
        /// Creates a new <typeparamref name="T" /> that is a deep copy of the current instance.
        /// </summary>
        /// <returns>The <typeparamref name="T" />.</returns>
        T DeepClone();
    }
}
