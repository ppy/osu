// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Screens
{
    internal class LoaderStorage : NamespacedResourceStore<byte[]>
    {
        public LoaderStorage(Storage storage)
            : base(new StorageBackedResourceStore(storage), "custom")
        {
        }
    }
}