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
        internal readonly TournamentVideoResourceStore VideoStore;
        private const string default_tournament = "default";

        public TournamentStorage(GameHost host)
            : base(host.Storage.GetStorageForDirectory("tournaments"), string.Empty)
        {
            this.host = host;

            TournamentStorageManager storageConfig = new TournamentStorageManager(host.Storage);

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

            VideoStore = new TournamentVideoResourceStore(this);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }

        internal void Migrate()
        {
            var source = new DirectoryInfo(host.Storage.GetFullPath("tournament"));
            var destination = new DirectoryInfo(GetFullPath(default_tournament));

            if (!destination.Exists)
                destination.Create();

            if (host.Storage.Exists("bracket.json"))
            {
                Logger.Log("Migrating bracket to default tournament storage.");
                var bracketFile = new System.IO.FileInfo(host.Storage.GetFullPath("bracket.json"));
                attemptOperation(() => bracketFile.CopyTo(Path.Combine(destination.FullName, bracketFile.Name), true));
                bracketFile.Delete();
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

    internal class TournamentVideoResourceStore : NamespacedResourceStore<byte[]>
    {
        public TournamentVideoResourceStore(Storage storage)
            : base(new StorageBackedResourceStore(storage), "videos")
        {
            AddExtension("m4v");
            AddExtension("avi");
            AddExtension("mp4");
        }
    }
}
