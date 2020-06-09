// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using System.IO;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament.IO
{
    internal class TournamentStorage : WrappedStorage
    {
        private readonly GameHost host;
        private readonly TournamentStorageManager storageConfig;
        public readonly TournamentVideoStorage VideoStorage;

        public TournamentStorage(GameHost host)
            : base(host.Storage.GetStorageForDirectory("tournaments"), string.Empty)
        {
            this.host = host;

            storageConfig = new TournamentStorageManager(host.Storage);
            var currentTournament = storageConfig.Get<string>(StorageConfig.CurrentTournament);

            if (!string.IsNullOrEmpty(currentTournament))
            {
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(currentTournament));
            }
            else
            {
                migrate();
                Logger.Log("Migrating files from old storage to new.");
            }

            VideoStorage = new TournamentVideoStorage(this);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }

        private void migrate()
        {
            const string default_path = "default";
            var source = new DirectoryInfo(host.Storage.GetFullPath("tournament"));
            var destination = new DirectoryInfo(GetFullPath(default_path));

            Directory.CreateDirectory(destination.FullName);

            if (host.Storage.Exists("bracket.json"))
            {
                Logger.Log("Migrating bracket to default tournament storage.");
                var bracketFile = new System.IO.FileInfo(host.Storage.GetFullPath("bracket.json"));
                attemptOperation(() => bracketFile.CopyTo(Path.Combine(destination.FullName, bracketFile.Name), true));
            }

            Logger.Log("Migrating other assets to default tournament storage.");
            copyRecursive(source, destination);
            ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(default_path));
            storageConfig.Set(StorageConfig.CurrentTournament, default_path);
            storageConfig.Save();
            deleteRecursive(source);
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
}
