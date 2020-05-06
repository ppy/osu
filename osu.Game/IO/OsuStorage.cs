// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : WrappedStorage
    {
        private readonly GameHost host;
        private readonly StorageConfigManager storageConfig;

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

            Directory.Move(oldLocation, newLocation);

            Directory.CreateDirectory(newLocation);
            // temporary
            Directory.CreateDirectory(oldLocation);

            // move back exceptions for now
            Directory.Move(Path.Combine(newLocation, "cache"), Path.Combine(oldLocation, "cache"));
            File.Move(Path.Combine(newLocation, "framework.ini"), Path.Combine(oldLocation, "framework.ini"));

            ChangeTargetStorage(host.GetStorage(newLocation));

            storageConfig.Set(StorageConfig.FullPath, newLocation);
            storageConfig.Save();
        }
    }
}
