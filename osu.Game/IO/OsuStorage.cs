// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : MigratableStorage
    {
        private readonly GameHost host;
        private readonly StorageConfigManager storageConfig;

        internal override string[] IGNORE_DIRECTORIES
        {
            get
            {
                return new string[] { "cache" };
            }
        }

        internal override string[] IGNORE_FILES
        {
            get
            {
                return new string[]
                {
                    "framework.ini",
                    "storage.ini"
                };
            }
        }

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

            // using Uri is the easiest way to check equality and contains (https://stackoverflow.com/a/7710620)
            var sourceUri = new Uri(source.FullName + Path.DirectorySeparatorChar);
            var destinationUri = new Uri(destination.FullName + Path.DirectorySeparatorChar);

            if (sourceUri == destinationUri)
                throw new ArgumentException("Destination provided is already the current location", nameof(newLocation));

            if (sourceUri.IsBaseOf(destinationUri))
                throw new ArgumentException("Destination provided is inside the source", nameof(newLocation));

            // ensure the new location has no files present, else hard abort
            if (destination.Exists)
            {
                if (destination.GetFiles().Length > 0 || destination.GetDirectories().Length > 0)
                    throw new ArgumentException("Destination provided already has files or directories present", nameof(newLocation));
            }

            CopyRecursive(source, destination);

            ChangeTargetStorage(host.GetStorage(newLocation));

            storageConfig.Set(StorageConfig.FullPath, newLocation);
            storageConfig.Save();

            DeleteRecursive(source);
        }
    }
}
