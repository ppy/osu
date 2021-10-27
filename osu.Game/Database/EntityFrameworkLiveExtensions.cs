// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Database
{
    public static class EntityFrameworkLiveExtensions
    {
        public static ILive<T> ToEntityFrameworkLive<T>(this T item)
            where T : class
        {
            return new EntityFrameworkLive<T>(item);
        }
    }
}
