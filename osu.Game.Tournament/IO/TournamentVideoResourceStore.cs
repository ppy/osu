// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Tournament.IO
{
    public class TournamentVideoResourceStore : NamespacedResourceStore<byte[]>
    {
        public TournamentVideoResourceStore(Storage storage)
            : base(new StorageBackedResourceStore(storage), "Videos")
        {
            AddExtension("m4v");
            AddExtension("avi");
            AddExtension("mp4");
        }
    }
}
