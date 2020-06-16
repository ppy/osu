// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using System.IO;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament.IO
{
    public class TournamentStorage : WrappedStorage
    {
        private readonly Storage storage;
        internal readonly TournamentVideoResourceStore VideoStore;
        internal readonly Storage ConfigurationStorage;
        private const string default_tournament = "default";
        private const string config_directory = "config";

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
                Migrate();
                storageConfig.Set(StorageConfig.CurrentTournament, default_tournament);
                storageConfig.Save();
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(default_tournament));
            }

            ConfigurationStorage = UnderlyingStorage.GetStorageForDirectory(config_directory);

            VideoStore = new TournamentVideoResourceStore(this);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }

        internal void Migrate()
        {
            var source = new DirectoryInfo(storage.GetFullPath("tournament"));
            var destination = new DirectoryInfo(GetFullPath(default_tournament));
            var cfgDestination = new DirectoryInfo(GetFullPath(default_tournament + Path.DirectorySeparatorChar + config_directory));

            if (!destination.Exists)
                destination.Create();

            if (!cfgDestination.Exists)
                destination.CreateSubdirectory(config_directory);

            if (storage.Exists("bracket.json"))
            {
                Logger.Log("Migrating bracket to default tournament storage.");
                var bracketFile = new System.IO.FileInfo(storage.GetFullPath("bracket.json"));
                moveFile(bracketFile, destination);
            }

            if (storage.Exists("drawings.txt"))
            {
                Logger.Log("Migrating drawings to default tournament storage.");
                var drawingsFile = new System.IO.FileInfo(storage.GetFullPath("drawings.txt"));
                moveFile(drawingsFile, destination);
            }

            if (storage.Exists("drawings.ini"))
            {
                Logger.Log("Migrating drawing configuration to default tournament storage.");
                var drawingsConfigFile = new System.IO.FileInfo(storage.GetFullPath("drawings.ini"));
                moveFile(drawingsConfigFile, cfgDestination);
            }

            if (storage.Exists("drawings_results.txt"))
            {
                Logger.Log("Migrating drawings results to default tournament storage.");
                var drawingsResultsFile = new System.IO.FileInfo(storage.GetFullPath("drawings_results.txt"));
                moveFile(drawingsResultsFile, destination);
            }

            if (source.Exists)
            {
                Logger.Log("Migrating tournament assets to default tournament storage.");
                copyRecursive(source, destination);
                deleteRecursive(source);
            }
        }

        private void copyRecursive(DirectoryInfo source, DirectoryInfo destination)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo

            foreach (System.IO.FileInfo fi in source.GetFiles())
            {
                attemptOperation(() => fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true));
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                copyRecursive(dir, destination.CreateSubdirectory(dir.Name));
            }
        }

        private void deleteRecursive(DirectoryInfo target)
        {
            foreach (System.IO.FileInfo fi in target.GetFiles())
            {
                attemptOperation(() => fi.Delete());
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                attemptOperation(() => dir.Delete(true));
            }

            if (target.GetFiles().Length == 0 && target.GetDirectories().Length == 0)
                attemptOperation(target.Delete);
        }

        private void moveFile(System.IO.FileInfo file, DirectoryInfo destination)
        {
            attemptOperation(() => file.CopyTo(Path.Combine(destination.FullName, file.Name), true));
            file.Delete();
        }

        private void attemptOperation(Action action, int attempts = 10)
        {
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception)
                {
                    if (attempts-- == 0)
                        throw;
                }

                Thread.Sleep(250);
            }
        }
    }
}
