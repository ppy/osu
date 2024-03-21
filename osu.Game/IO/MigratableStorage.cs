// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Utils;

namespace osu.Game.IO
{
    /// <summary>
    /// A <see cref="WrappedStorage"/> that is migratable to different locations.
    /// </summary>
    public abstract class MigratableStorage : WrappedStorage
    {
        /// <summary>
        /// A relative list of directory paths which should not be migrated.
        /// </summary>
        public virtual string[] IgnoreDirectories => Array.Empty<string>();

        /// <summary>
        /// A relative list of file paths which should not be migrated.
        /// </summary>
        public virtual string[] IgnoreFiles => Array.Empty<string>();

        /// <summary>
        /// A list of file/directory suffixes which should not be migrated.
        /// </summary>
        public virtual string[] IgnoreSuffixes => Array.Empty<string>();

        protected MigratableStorage(Storage storage, string subPath = null)
            : base(storage, subPath)
        {
        }

        /// <summary>
        /// A general purpose migration method to move the storage to a different location.
        /// <param name="newStorage">The target storage of the migration.</param>
        /// </summary>
        /// <returns>Whether cleanup could complete.</returns>
        public virtual bool Migrate(Storage newStorage)
        {
            var source = new DirectoryInfo(GetFullPath("."));
            var destination = new DirectoryInfo(newStorage.GetFullPath("."));

            // using Uri is the easiest way to check equality and contains (https://stackoverflow.com/a/7710620)
            var sourceUri = new Uri(source.FullName + Path.DirectorySeparatorChar);
            var destinationUri = new Uri(destination.FullName + Path.DirectorySeparatorChar);

            if (sourceUri == destinationUri)
                throw new ArgumentException("Destination provided is already the current location", destination.FullName);

            if (sourceUri.IsBaseOf(destinationUri))
                throw new ArgumentException("Destination provided is inside the source", destination.FullName);

            // ensure the new location has no files present, else hard abort
            if (destination.Exists)
            {
                if (destination.GetFiles().Length > 0 || destination.GetDirectories().Length > 0)
                    throw new ArgumentException("Destination provided already has files or directories present", destination.FullName);
            }

            CopyRecursive(source, destination);
            ChangeTargetStorage(newStorage);

            return DeleteRecursive(source);
        }

        protected bool DeleteRecursive(DirectoryInfo target, bool topLevelExcludes = true)
        {
            bool allFilesDeleted = true;

            foreach (System.IO.FileInfo fi in target.GetFiles())
            {
                if (topLevelExcludes && IgnoreFiles.Contains(fi.Name))
                    continue;

                if (IgnoreSuffixes.Any(suffix => fi.Name.EndsWith(suffix, StringComparison.Ordinal)))
                    continue;

                allFilesDeleted &= FileUtils.AttemptOperation(() => fi.Delete(), throwOnFailure: false);
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                if (topLevelExcludes && IgnoreDirectories.Contains(dir.Name))
                    continue;

                if (IgnoreSuffixes.Any(suffix => dir.Name.EndsWith(suffix, StringComparison.Ordinal)))
                    continue;

                allFilesDeleted &= FileUtils.AttemptOperation(() => dir.Delete(true), throwOnFailure: false);
            }

            if (target.GetFiles().Length == 0 && target.GetDirectories().Length == 0)
                allFilesDeleted &= FileUtils.AttemptOperation(target.Delete, throwOnFailure: false);

            return allFilesDeleted;
        }

        protected void CopyRecursive(DirectoryInfo source, DirectoryInfo destination, bool topLevelExcludes = true)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo
            if (!destination.Exists)
                Directory.CreateDirectory(destination.FullName);

            foreach (System.IO.FileInfo fileInfo in source.GetFiles())
            {
                if (topLevelExcludes && IgnoreFiles.Contains(fileInfo.Name))
                    continue;

                if (IgnoreSuffixes.Any(suffix => fileInfo.Name.EndsWith(suffix, StringComparison.Ordinal)))
                    continue;

                FileUtils.AttemptOperation(() =>
                {
                    fileInfo.Refresh();

                    // A temporary file may have been deleted since the initial GetFiles operation.
                    // We don't want the whole migration process to fail in such a case.
                    if (!fileInfo.Exists)
                        return;

                    fileInfo.CopyTo(Path.Combine(destination.FullName, fileInfo.Name), true);
                });
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (topLevelExcludes && IgnoreDirectories.Contains(dir.Name))
                    continue;

                if (IgnoreSuffixes.Any(suffix => dir.Name.EndsWith(suffix, StringComparison.Ordinal)))
                    continue;

                CopyRecursive(dir, destination.CreateSubdirectory(dir.Name), false);
            }
        }
    }
}
