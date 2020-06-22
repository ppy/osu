// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using System.IO;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament.IO
{
    public class TournamentStorage : MigratableStorage
    {
        private readonly Storage storage;
        internal readonly TournamentVideoResourceStore VideoStore;
        private const string default_tournament = "default";

        public TournamentStorage(Storage storage)
            : base(storage.GetStorageForDirectory("tournaments"), string.Empty)
        {
            this.storage = storage;

            TournamentStorageManager storageConfig = new TournamentStorageManager(storage);

            var currentTournament = storageConfig.Get<string>(StorageConfig.CurrentTournament);

            if (!string.IsNullOrEmpty(currentTournament))
            {
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(currentTournament));
            }
            else
            {
                migrate();
                storageConfig.Set(StorageConfig.CurrentTournament, default_tournament);
                storageConfig.Save();
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(default_tournament));
            }

            VideoStore = new TournamentVideoResourceStore(this);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }

        private void migrate()
        {
            var source = new DirectoryInfo(storage.GetFullPath("tournament"));
            var destination = new DirectoryInfo(GetFullPath(default_tournament));

            if (source.Exists)
            {
                Logger.Log("Migrating tournament assets to default tournament storage.");
                CopyRecursive(source, destination);
                DeleteRecursive(source);
            }

            moveFileIfExists("bracket.json", destination);
            moveFileIfExists("drawings.txt", destination);
            moveFileIfExists("drawings_results.txt", destination);
            moveFileIfExists("drawings.ini", destination);
        }

        private void moveFileIfExists(string file, DirectoryInfo destination)
        {
            if (storage.Exists(file))
            {
                Logger.Log($"Migrating {file} to default tournament storage.");
                var fileInfo = new System.IO.FileInfo(storage.GetFullPath(file));
                AttemptOperation(() => fileInfo.CopyTo(Path.Combine(destination.FullName, fileInfo.Name), true));
                fileInfo.Delete();
            }
        }
    }
}
