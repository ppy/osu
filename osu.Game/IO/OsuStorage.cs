// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
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
            {
                ChangeTargetStorage(host.GetStorage(customStoragePath));
                Logger.Storage = UnderlyingStorage.GetStorageForDirectory("logs");
            }
        }

        public void Migrate(string newLocation)
        {
            string oldLocation = GetFullPath(".");

            // ensure the new location has no files present, else hard abort
            if (Directory.Exists(newLocation))
            {
                if (Directory.GetFiles(newLocation).Length > 0)
                    throw new InvalidOperationException("Migration destination already has files present");

                Directory.Delete(newLocation, true);
            }

            var source = new DirectoryInfo(oldLocation);
            var destination = new DirectoryInfo(newLocation);

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
                if (IGNORE_FILES.Contains(fi.Name))
                    continue;

                fi.Delete();
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                if (IGNORE_DIRECTORIES.Contains(dir.Name))
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
                if (IGNORE_FILES.Contains(fi.Name))
                    continue;

                fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (IGNORE_DIRECTORIES.Contains(dir.Name))
                    continue;

                copyRecursive(dir, destination.CreateSubdirectory(dir.Name), false);
            }
        }
    }
}
