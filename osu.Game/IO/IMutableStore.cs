// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.IO
{
    public interface IMutableStore<in T>
    {
        /// <summary>
        /// Add an object to the store.
        /// </summary>
        /// <param name="object">The object to add.</param>
        void Add(T item);

        bool Delete(T item);
    }
}
