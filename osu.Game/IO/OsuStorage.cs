// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : WrappedStorage
    {
        /// <summary>
        /// Indicates the error (if any) that occurred when initialising the custom storage during initial startup.
        /// </summary>
        public readonly OsuStorageError Error;

        /// <summary>
        /// The custom storage path as selected by the user.
        /// </summary>
        [CanBeNull]
        public string CustomStoragePath => storageConfig.Get<string>(StorageConfig.FullPath);

        /// <summary>
        /// The default storage path to be used if a custom storage path hasn't been selected or is not accessible.
        /// </summary>
        [NotNull]
        public string DefaultStoragePath => defaultStorage.GetFullPath(".");

        private readonly GameHost host;
        private readonly StorageConfigManager storageConfig;
        private readonly Storage defaultStorage;

        public static readonly string[] IGNORE_DIRECTORIES = { "cache" };

        public static readonly string[] IGNORE_FILES =
        {
            "framework.ini",
            "storage.ini"
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

        public Storage GetStorageFromPath(string path) =>
            IGNORE_DIRECTORIES.Any(x => path.StartsWith(x)) ? defaultStorage : UnderlyingStorage;

        public override string GetFullPath(string path, bool createIfNotExisting = false) =>
            GetStorageFromPath(path).GetFullPath(MutatePath(path), createIfNotExisting);

        public override bool Exists(string path) =>
            GetStorageFromPath(path).Exists(MutatePath(path));

        public override bool ExistsDirectory(string path) =>
            GetStorageFromPath(path).ExistsDirectory(MutatePath(path));

        public override void DeleteDirectory(string path) =>
            GetStorageFromPath(path).DeleteDirectory(MutatePath(path));

        public override void Delete(string path) =>
            GetStorageFromPath(path).Delete(MutatePath(path));

        public override IEnumerable<string> GetDirectories(string path) =>
            ToLocalRelative(GetStorageFromPath(path).GetDirectories(MutatePath(path)));

        public override IEnumerable<string> ToLocalRelative(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                Storage storage = GetStorageFromPath(path);
                string localRoot = storage.GetFullPath(string.Empty);
                yield return Path.GetRelativePath(localRoot, storage.GetFullPath(path));
            }
        }

        public override IEnumerable<string> GetFiles(string path, string pattern = "*") =>
            ToLocalRelative(GetStorageFromPath(path).GetFiles(MutatePath(path), pattern));

        public override Stream GetStream(string path, FileAccess access = FileAccess.Read, FileMode mode = FileMode.OpenOrCreate) =>
            GetStorageFromPath(path).GetStream(MutatePath(path), access, mode);

        public override void OpenPathInNativeExplorer(string path) => GetStorageFromPath(path).OpenPathInNativeExplorer(MutatePath(path));

        public override Storage GetStorageForDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Must be non-null and not empty string", nameof(path));

            if (!path.EndsWith(Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;

            // create non-existing path.
            GetStorageFromPath(path).GetFullPath(path, true);

            return new WrappedStorage(this, path);
        }

        /// <summary>
        /// Resets the custom storage path, changing the target storage to the default location.
        /// </summary>
        public void ResetCustomStoragePath()
        {
            storageConfig.Set(StorageConfig.FullPath, string.Empty);
            storageConfig.Save();

            ChangeTargetStorage(defaultStorage);
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

            try
            {
                Storage userStorage = host.GetStorage(CustomStoragePath);

                if (!userStorage.ExistsDirectory(".") || !userStorage.GetFiles(".").Any())
                    error = OsuStorageError.AccessibleButEmpty;

                ChangeTargetStorage(userStorage);
            }
            catch
            {
                error = OsuStorageError.NotAccessible;
                ChangeTargetStorage(lastStorage);
            }

            return error == OsuStorageError.None;
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

                attemptOperation(() => fi.Delete());
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                if (topLevelExcludes && IGNORE_DIRECTORIES.Contains(dir.Name))
                    continue;

                attemptOperation(() => dir.Delete(true));
            }

            if (target.GetFiles().Length == 0 && target.GetDirectories().Length == 0)
                attemptOperation(target.Delete);
        }

        private static void copyRecursive(DirectoryInfo source, DirectoryInfo destination, bool topLevelExcludes = true)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo
            Directory.CreateDirectory(destination.FullName);

            foreach (System.IO.FileInfo fi in source.GetFiles())
            {
                if (topLevelExcludes && IGNORE_FILES.Contains(fi.Name))
                    continue;

                attemptOperation(() => fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true));
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (topLevelExcludes && IGNORE_DIRECTORIES.Contains(dir.Name))
                    continue;

                copyRecursive(dir, destination.CreateSubdirectory(dir.Name), false);
            }
        }

        /// <summary>
        /// Attempt an IO operation multiple times and only throw if none of the attempts succeed.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="attempts">The number of attempts (250ms wait between each).</param>
        private static void attemptOperation(Action action, int attempts = 10)
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
