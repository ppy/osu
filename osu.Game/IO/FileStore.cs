// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using SQLite.Net;

namespace osu.Game.IO
{
    /// <summary>
    /// Handles the Store and retrieval of Files/FileSets to the database backing
    /// </summary>
    public class FileStore : DatabaseBackedStore
    {
        private const string prefix = "files";

        public readonly ResourceStore<byte[]> Store;

        protected override int StoreVersion => 2;

        public FileStore(SQLiteConnection connection, Storage storage) : base(connection, storage)
        {
            Store = new NamespacedResourceStore<byte[]>(new StorageBackedResourceStore(storage), prefix);
        }

        protected override Type[] ValidTypes => new[] {
            typeof(FileInfo),
        };

        protected override void Prepare(bool reset = false)
        {
            if (reset)
            {
                try
                {
                    foreach (var f in Query<FileInfo>())
                        Storage.Delete(Path.Combine(prefix, f.StoragePath));
                }
                catch
                {
                    // we don't want to ever crash as a result of a reset operation.
                }

                Connection.DropTable<FileInfo>();
            }

            Connection.CreateTable<FileInfo>();
        }

        protected override void StartupTasks()
        {
            base.StartupTasks();
            deletePending();
        }

        /// <summary>
        /// Perform migrations between two store versions.
        /// </summary>
        /// <param name="currentVersion">The current store version. This will be zero on a fresh database initialisation.</param>
        /// <param name="targetVersion">The target version which we are migrating to (equal to the current <see cref="StoreVersion"/>).</param>
        protected override void PerformMigration(int currentVersion, int targetVersion)
        {
            base.PerformMigration(currentVersion, targetVersion);

            while (currentVersion++ < targetVersion)
            {
                switch (currentVersion)
                {
                    case 1:
                    case 2:
                        // cannot migrate; breaking underlying changes.
                        Reset();
                        break;
                }
            }
        }

        public FileInfo Add(Stream data)
        {
            string hash = data.ComputeSHA2Hash();

            var info = new FileInfo { Hash = hash };

            var existing = Connection.Table<FileInfo>().FirstOrDefault(f => f.Hash == info.Hash);

            if (existing != null)
            {
                info = existing;
            }
            else
            {
                string path = Path.Combine(prefix, info.StoragePath);

                data.Seek(0, SeekOrigin.Begin);

                if (!Storage.Exists(path))
                    using (var output = Storage.GetStream(path, FileAccess.Write))
                        data.CopyTo(output);

                data.Seek(0, SeekOrigin.Begin);

                Connection.Insert(info);
            }

            Reference(new[] { info });
            return info;
        }

        public void Reference(IEnumerable<FileInfo> files)
        {
            Connection.RunInTransaction(() =>
            {
                var incrementedFiles = files.GroupBy(f => f.ID).Select(f =>
                {
                    var accurateRefCount = Connection.Get<FileInfo>(f.First().ID);
                    accurateRefCount.ReferenceCount += f.Count();
                    return accurateRefCount;
                });

                Connection.UpdateAll(incrementedFiles);
            });
        }

        public void Dereference(IEnumerable<FileInfo> files)
        {
            Connection.RunInTransaction(() =>
            {
                var incrementedFiles = files.GroupBy(f => f.ID).Select(f =>
                {
                    var accurateRefCount = Connection.Get<FileInfo>(f.First().ID);
                    accurateRefCount.ReferenceCount -= f.Count();
                    return accurateRefCount;
                });

                Connection.UpdateAll(incrementedFiles);
            });
        }

        private void deletePending()
        {
            foreach (var f in QueryAndPopulate<FileInfo>(f => f.ReferenceCount < 1))
            {
                try
                {
                    Connection.Delete(f);
                    Storage.Delete(Path.Combine(prefix, f.StoragePath));
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not delete beatmap {f}");
                }
            }
        }
    }
}