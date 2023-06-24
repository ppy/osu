// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : MigratableStorage
    {
        /// <summary>
        /// Indicates the error (if any) that occurred when initialising the custom storage during initial startup.
        /// </summary>
        public readonly OsuStorageError Error;

        /// <summary>
        /// The custom storage path as selected by the user.
        /// </summary>
        public string? CustomStoragePath => storageConfig.Get<string>(StorageConfig.FullPath);

        /// <summary>
        /// The default storage path to be used if a custom storage path hasn't been selected or is not accessible.
        /// </summary>
        public string DefaultStoragePath => defaultStorage.GetFullPath(".");

        private readonly GameHost host;
        private readonly StorageConfigManager storageConfig;
        private readonly Storage defaultStorage;

        public override string[] IgnoreDirectories => new[]
        {
            "cache",
        };

        public override string[] IgnoreFiles => new[]
        {
            "framework.ini",
            "storage.ini",
        };

        public override string[] IgnoreSuffixes => new[]
        {
            // Realm pipe files don't play well with copy operations
            ".note",
            ".lock",
            ".management",
        };

        public OsuStorage(GameHost host, Storage defaultStorage)
            : base(defaultStorage, string.Empty)
        {
            this.host = host;
            this.defaultStorage = defaultStorage;

            storageConfig = new StorageConfigManager(defaultStorage);

            if (!string.IsNullOrEmpty(CustomStoragePath))
                TryChangeToCustomStorage(out Error);
        }

        /// <summary>
        /// Resets the custom storage path, changing the target storage to the default location.
        /// </summary>
        public void ResetCustomStoragePath()
        {
            ChangeDataPath(string.Empty);

            ChangeTargetStorage(defaultStorage);
        }

        /// <summary>
        /// Updates the target data path without immediately switching.
        /// This does NOT migrate any data.
        /// The game should immediately be restarted after calling this.
        /// </summary>
        public void ChangeDataPath(string newPath)
        {
            storageConfig.SetValue(StorageConfig.FullPath, newPath);
            storageConfig.Save();
        }

        /// <summary>
        /// Attempts to change to the user's custom storage path.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        /// <returns>Whether the custom storage path was used successfully. If not, <paramref name="error"/> will be populated with the reason.</returns>
        public bool TryChangeToCustomStorage(out OsuStorageError error)
        {
            Debug.Assert(!string.IsNullOrEmpty(CustomStoragePath));

            error = OsuStorageError.None;
            Storage lastStorage = UnderlyingStorage;

            Logger.Log($"Attempting to use custom storage location {CustomStoragePath}");

            try
            {
                Storage userStorage = host.GetStorage(CustomStoragePath);

                if (!userStorage.ExistsDirectory(".") || !userStorage.GetFiles(".").Any())
                    error = OsuStorageError.AccessibleButEmpty;

                ChangeTargetStorage(userStorage);
                Logger.Log($"Storage successfully changed to {CustomStoragePath}.");
            }
            catch
            {
                error = OsuStorageError.NotAccessible;
                ChangeTargetStorage(lastStorage);
            }

            if (error != OsuStorageError.None)
                Logger.Log($"Custom storage location could not be used ({error}).");

            return error == OsuStorageError.None;
        }

        protected override void ChangeTargetStorage(Storage newStorage)
        {
            var lastStorage = UnderlyingStorage;
            base.ChangeTargetStorage(newStorage);

            if (lastStorage != null)
            {
                // for now we assume that if there was a previous storage, this is a migration operation.
                // the logger shouldn't be set during initialisation as it can cause cross-talk in tests (due to being static).
                Logger.Storage = UnderlyingStorage.GetStorageForDirectory("logs");
            }
        }

        public override bool Migrate(Storage newStorage)
        {
            bool cleanupSucceeded = base.Migrate(newStorage);

            ChangeDataPath(newStorage.GetFullPath("."));

            return cleanupSucceeded;
        }
    }

    public enum OsuStorageError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None,

        /// <summary>
        /// Occurs when the target storage directory is accessible but does not already contain game files.
        /// Only happens when the user changes the storage directory and then moves the files manually or mounts a different device to the same path.
        /// </summary>
        AccessibleButEmpty,

        /// <summary>
        /// Occurs when the target storage directory cannot be accessed at all.
        /// </summary>
        NotAccessible,
    }
}
