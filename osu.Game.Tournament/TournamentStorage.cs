// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Tournament
{
    internal class TournamentStorage : NamespacedResourceStore<byte[]>
    {
        public TournamentStorage(Storage storage)
            : base(new StorageBackedResourceStore(storage), "tournament")
        {
            AddExtension("m4v");
            AddExtension("avi");
            AddExtension("mp4");
        }
    }
}
