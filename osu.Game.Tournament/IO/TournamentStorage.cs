// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament.IO
{
    public class TournamentStorage : MigratableStorage
    {
        private const string default_tournament = "default";
        private readonly Storage storage;

        /// <summary>
        /// The storage where all tournaments are located.
        /// </summary>
        public readonly Storage AllTournaments;

        private readonly TournamentStorageManager storageConfig;
        public readonly Bindable<string> CurrentTournament;

        public TournamentStorage(Storage storage)
            : base(storage.GetStorageForDirectory("tournaments"), string.Empty)
        {
            this.storage = storage;
            AllTournaments = UnderlyingStorage;

            storageConfig = new TournamentStorageManager(storage);

            if (storage.Exists("tournament.ini"))
            {
                ChangeTargetStorage(AllTournaments.GetStorageForDirectory(storageConfig.Get<string>(StorageConfig.CurrentTournament)));
            }
            else
                Migrate(AllTournaments.GetStorageForDirectory(default_tournament));

            CurrentTournament = storageConfig.GetBindable<string>(StorageConfig.CurrentTournament);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));

            CurrentTournament.BindValueChanged(updateTournament);
        }

        private void updateTournament(ValueChangedEvent<string> newTournament)
        {
            ChangeTargetStorage(AllTournaments.GetStorageForDirectory(newTournament.NewValue));
            Logger.Log("Changing tournament storage: " + GetFullPath(string.Empty));
        }

        public IEnumerable<string> ListTournaments() => AllTournaments.GetDirectories(string.Empty);

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
            storageConfig.SetValue(StorageConfig.CurrentTournament, default_tournament);
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
