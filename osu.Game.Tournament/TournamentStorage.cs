// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament
{
    internal class TournamentVideoStorage : NamespacedResourceStore<byte[]>
    {
        public TournamentVideoStorage(Storage storage)
            : base(new StorageBackedResourceStore(storage), "videos")
        {
            AddExtension("m4v");
            AddExtension("avi");
            AddExtension("mp4");
        }
    }

    internal class NewTournamentStorage : WrappedStorage
    {
        private readonly GameHost host;
        private readonly TournamentStorageManager storageConfig;
        public readonly TournamentVideoStorage VideoStorage;

        public NewTournamentStorage(GameHost host)
            : base(host.Storage, string.Empty)
        {
            this.host = host;

            storageConfig = new TournamentStorageManager(host.Storage);
            var customTournamentPath = storageConfig.Get<string>(StorageConfig.CurrentTournament);

            if (!string.IsNullOrEmpty(customTournamentPath))
            {
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory("tournaments/" + customTournamentPath));
            } else {
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory("tournaments/default"));
            }
            VideoStorage = new TournamentVideoStorage(this);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }
    }
}
