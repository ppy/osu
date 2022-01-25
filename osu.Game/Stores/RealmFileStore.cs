// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;
using Realms;

#nullable enable

namespace osu.Game.Stores
{
    /// <summary>
    /// Handles the storing of files to the file system (and database) backing.
    /// </summary>
    [ExcludeFromDynamicCompile]
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
        /// <returns></returns>
        public RealmFile Add(Stream data, Realm realm)
        {
            string hash = data.ComputeSHA2Hash();

            var existing = realm.Find<RealmFile>(hash);

            var file = existing ?? new RealmFile { Hash = hash };

            if (!checkFileExistsAndMatchesHash(file))
                copyToStore(file, data);

            if (!file.IsManaged)
                realm.Add(file);

            return file;
        }

        private void copyToStore(RealmFile file, Stream data)
        {
            data.Seek(0, SeekOrigin.Begin);

            using (var output = Storage.GetStream(file.GetStoragePath(), FileAccess.Write))
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
                var files = r.All<RealmFile>().ToList();

                foreach (var file in files)
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
