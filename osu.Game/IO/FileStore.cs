// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
                // in earlier versions we stored beatmaps as solid archives, but not any more.
                if (Storage.ExistsDirectory("beatmaps"))
                    Storage.DeleteDirectory("beatmaps");

                if (Storage.ExistsDirectory(prefix))
                    Storage.DeleteDirectory(prefix);

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

            var existing = Connection.Table<FileInfo>().Where(f => f.Hash == hash).FirstOrDefault();

            var info = existing ?? new FileInfo { Hash = hash };
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

            Reference(info);
            return info;
        }

        public void Reference(params FileInfo[] files)
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

        public void Dereference(params FileInfo[] files)
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
            Connection.RunInTransaction(() =>
            {
                foreach (var f in Query<FileInfo>(f => f.ReferenceCount < 1))
                {
                    try
                    {
                        Storage.Delete(Path.Combine(prefix, f.StoragePath));
                        Connection.Delete(f);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete beatmap {f}");
                    }
                }
            });
        }
    }
}