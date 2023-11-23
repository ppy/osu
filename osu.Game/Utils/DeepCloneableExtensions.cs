// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public static class DeepCloneableExtensions
    {
        /// <remarks>
        /// This method has the same signature as the default implementation of <see cref="IDeepCloneable{T}.DeepClone()"/>,
        /// however, default implementations are only available when a variable is typed as IDeepCloneable{T} directly but
        /// becomes unavailable the moment the variable is typed as a class that implements IDeepCloneable{T}.
        /// This extension method allows the default implementation to be used in both cases.
        /// </remarks>
        public static T DeepClone<T>(this T obj)
            where T : class, IDeepCloneable<T>
        {
            return obj.DeepClone();
        }
    }
}
