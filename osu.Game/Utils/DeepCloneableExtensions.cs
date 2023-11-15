// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public static class DeepCloneableExtensions
    {
        public static T DeepClone<T>(this T obj)
            where T : class, IDeepCloneable<T>
        {
            return obj.DeepClone();
        }
    }
}
