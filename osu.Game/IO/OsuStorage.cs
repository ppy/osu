// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : WrappedStorage
    {
        private readonly GameHost host;
        private readonly StorageConfigManager storageConfig;

        internal static readonly string[] IGNORE_DIRECTORIES = { "cache" };

        internal static readonly string[] IGNORE_FILES =
        {
            "framework.ini",
            "storage.ini"
        };

        public OsuStorage(GameHost host)
            : base(host.Storage, string.Empty)
        {
            this.host = host;

            storageConfig = new StorageConfigManager(host.Storage);

            var customStoragePath = storageConfig.Get<string>(StorageConfig.FullPath);

            if (!string.IsNullOrEmpty(customStoragePath))
                ChangeTargetStorage(host.GetStorage(customStoragePath));
        }

        protected override void ChangeTargetStorage(Storage newStorage)
        {
            base.ChangeTargetStorage(newStorage);
            Logger.Storage = UnderlyingStorage.GetStorageForDirectory("logs");
        }

        public void Migrate(string newLocation)
        {
            var source = new DirectoryInfo(GetFullPath("."));
            var destination = new DirectoryInfo(newLocation);

            if (source.FullName == destination.FullName)
                throw new ArgumentException("Destination provided is already the current location", nameof(newLocation));

            if (destination.FullName.Contains(source.FullName))
                throw new ArgumentException("Destination provided is inside the source", nameof(newLocation));

            // ensure the new location has no files present, else hard abort
            if (destination.Exists)
            {
                if (destination.GetFiles().Length > 0 || destination.GetDirectories().Length > 0)
                    throw new InvalidOperationException("Migration destination already has files present");

                deleteRecursive(destination);
            }

            copyRecursive(source, destination);

            ChangeTargetStorage(host.GetStorage(newLocation));

            storageConfig.Set(StorageConfig.FullPath, newLocation);
            storageConfig.Save();

            deleteRecursive(source);
        }

        private static void deleteRecursive(DirectoryInfo target, bool topLevelExcludes = true)
        {
            foreach (System.IO.FileInfo fi in target.GetFiles())
            {
                if (topLevelExcludes && IGNORE_FILES.Contains(fi.Name))
                    continue;

                fi.Delete();
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                if (topLevelExcludes && IGNORE_DIRECTORIES.Contains(dir.Name))
                    continue;

                dir.Delete(true);
            }
        }

        private static void copyRecursive(DirectoryInfo source, DirectoryInfo destination, bool topLevelExcludes = true)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo
            Directory.CreateDirectory(destination.FullName);

            foreach (System.IO.FileInfo fi in source.GetFiles())
            {
                if (topLevelExcludes && IGNORE_FILES.Contains(fi.Name))
                    continue;

                attemptCopy(fi, Path.Combine(destination.FullName, fi.Name));
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (topLevelExcludes && IGNORE_DIRECTORIES.Contains(dir.Name))
                    continue;

                copyRecursive(dir, destination.CreateSubdirectory(dir.Name), false);
            }
        }

        private static void attemptCopy(System.IO.FileInfo fileInfo, string destination)
        {
            int tries = 5;

            while (true)
            {
                try
                {
                    fileInfo.CopyTo(destination, true);
                    return;
                }
                catch (Exception)
                {
                    if (tries-- == 0)
                        throw;
                }

                Thread.Sleep(50);
            }
        }
    }
}
