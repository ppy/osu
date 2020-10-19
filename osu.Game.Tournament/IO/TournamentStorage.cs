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
        private const string default_tournament = "default";
        private readonly Storage storage;
        private readonly TournamentStorageManager storageConfig;

        public TournamentStorage(Storage storage)
            : base(storage.GetStorageForDirectory("tournaments"), string.Empty)
        {
            this.storage = storage;

            storageConfig = new TournamentStorageManager(storage);

            if (storage.Exists("tournament.ini"))
            {
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(storageConfig.Get<string>(StorageConfig.CurrentTournament)));
            }
            else
                Migrate(UnderlyingStorage.GetStorageForDirectory(default_tournament));

            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }

        public override void Migrate(Storage newStorage)
        {
            // this migration only happens once on moving to the per-tournament storage system.
            // listed files are those known at that point in time.
            // this can be removed at some point in the future (6 months obsoletion would mean 2021-04-19)

            var source = new DirectoryInfo(storage.GetFullPath("tournament"));
            var destination = new DirectoryInfo(newStorage.GetFullPath("."));

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

            ChangeTargetStorage(newStorage);
            storageConfig.Set(StorageConfig.CurrentTournament, default_tournament);
            storageConfig.Save();
        }

        private void moveFileIfExists(string file, DirectoryInfo destination)
        {
            if (!storage.Exists(file))
                return;

            Logger.Log($"Migrating {file} to default tournament storage.");
            var fileInfo = new System.IO.FileInfo(storage.GetFullPath(file));
            AttemptOperation(() => fileInfo.CopyTo(Path.Combine(destination.FullName, fileInfo.Name), true));
            fileInfo.Delete();
        }
    }
}
