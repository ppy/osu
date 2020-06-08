// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using System.IO;
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

    internal class TournamentStorage : WrappedStorage
    {
        private readonly GameHost host;
        private readonly TournamentStorageManager storageConfig;
        public readonly TournamentVideoStorage VideoStorage;

        public TournamentStorage(GameHost host)
            : base(host.Storage, string.Empty)
        {
            this.host = host;

            storageConfig = new TournamentStorageManager(host.Storage);
            var currentTournament = storageConfig.Get<string>(StorageConfig.CurrentTournament);

            if (!string.IsNullOrEmpty(currentTournament))
            {
                ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory("tournaments" + Path.DirectorySeparatorChar + currentTournament));
            }
            else
            {
                // Migrating old storage format to the new one.
                migrate();
                Logger.Log("Migrating files from old storage to new.");
            }

            VideoStorage = new TournamentVideoStorage(this);
            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));
        }

        private void migrate()
        {
            const string default_path = "tournaments/default";
            var source = new DirectoryInfo(GetFullPath("tournament"));
            var destination = new DirectoryInfo(GetFullPath(default_path));

            Directory.CreateDirectory(destination.FullName);

            if (host.Storage.Exists("bracket.json"))
            {
                Logger.Log("Migrating bracket to default tournament storage.");
                var bracketFile = new System.IO.FileInfo(GetFullPath(string.Empty) + Path.DirectorySeparatorChar + GetFiles(string.Empty, "bracket.json").First());
                attemptOperation(() => bracketFile.CopyTo(Path.Combine(destination.FullName, bracketFile.Name), true));
            }

            Logger.Log("Migrating other assets to default tournament storage.");
            copyRecursive(source, destination);
            ChangeTargetStorage(UnderlyingStorage.GetStorageForDirectory(default_path));
            storageConfig.Set(StorageConfig.CurrentTournament, default_path);
            storageConfig.Save();
        }

        private void copyRecursive(DirectoryInfo source, DirectoryInfo destination, bool topLevelExcludes = true)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo

            foreach (System.IO.FileInfo fi in source.GetFiles())
            {
                attemptOperation(() => fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true));
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                copyRecursive(dir, destination.CreateSubdirectory(dir.Name), false);
            }
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
