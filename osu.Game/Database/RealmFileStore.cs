// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Models;
using Realms;

namespace osu.Game.Database
{
    /// <summary>
    /// Handles the storing of files to the file system (and database) backing.
    /// </summary>
    public class RealmFileStore
    {
        private readonly RealmAccess realm;

        public readonly IResourceStore<byte[]> Store;

        public readonly Storage Storage;

        public RealmFileStore(RealmAccess realm, Storage storage)
        {
            this.realm = realm;

            Storage = storage.GetStorageForDirectory(@"files");
            Store = new StorageBackedResourceStore(Storage);
        }

        /// <summary>
        /// Add a new file to the game-wide database, copying it to permanent storage if not already present.
        /// </summary>
        /// <param name="data">The file data stream.</param>
        /// <param name="realm">The realm instance to add to. Should already be in a transaction.</param>
        /// <param name="addToRealm">Whether the <see cref="RealmFile"/> should immediately be added to the underlying realm. If <c>false</c> is provided here, the instance must be manually added.</param>
        /// <param name="preferHardLinks">Whether this import should use hard links rather than file copy operations if available.</param>
        public RealmFile Add(Stream data, Realm realm, bool addToRealm = true, bool preferHardLinks = false)
        {
            string hash = data.ComputeSHA2Hash();

            var existing = realm.Find<RealmFile>(hash);

            var file = existing ?? new RealmFile { Hash = hash };

            if (!checkFileExistsAndMatchesHash(file))
                copyToStore(file, data, preferHardLinks);

            if (addToRealm && !file.IsManaged)
                realm.Add(file);

            return file;
        }

        private void copyToStore(RealmFile file, Stream data, bool preferHardLinks)
        {
            if (data is FileStream fs && preferHardLinks)
            {
                // attempt to do a fast hard link rather than copy.
                if (HardLinkHelper.TryCreateHardLink(Storage.GetFullPath(file.GetStoragePath(), true), fs.Name))
                    return;
            }

            data.Seek(0, SeekOrigin.Begin);

            using (var output = Storage.CreateFileSafely(file.GetStoragePath()))
                data.CopyTo(output);

            data.Seek(0, SeekOrigin.Begin);
        }

        private bool checkFileExistsAndMatchesHash(RealmFile file)
        {
            string path = file.GetStoragePath();

            // we may be re-adding a file to fix missing store entries.
            if (!Storage.Exists(path))
                return false;

            // even if the file already exists, check the existing checksum for safety.
            using (var stream = Storage.GetStream(path))
                return stream.ComputeSHA2Hash() == file.Hash;
        }

        public void Cleanup()
        {
            Logger.Log(@"Beginning realm file store cleanup");

            int totalFiles = 0;
            int removedFiles = 0;

            // can potentially be run asynchronously, although we will need to consider operation order for disk deletion vs realm removal.
            realm.Write(r =>
            {
                // TODO: consider using a realm native query to avoid iterating all files (https://github.com/realm/realm-dotnet/issues/2659#issuecomment-927823707)
                foreach (var file in r.All<RealmFile>())
                {
                    totalFiles++;

                    if (file.BacklinksCount > 0)
                        continue;

                    try
                    {
                        removedFiles++;
                        Storage.Delete(file.GetStoragePath());
                        r.Remove(file);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete databased file {file.Hash}");
                    }
                }
            });

            Logger.Log($@"Finished realm file store cleanup ({removedFiles} of {totalFiles} deleted)");
        }
    }
}
