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
                Connection.DropTable<FileInfo>();

            Connection.CreateTable<FileInfo>();

            deletePending();
        }

        public FileInfo Add(Stream data, string filename = null)
        {
            string hash = data.ComputeSHA2Hash();

            var info = new FileInfo
            {
                Filename = filename,
                Hash = hash,
            };

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
            foreach (var f in files)
            {
                f.ReferenceCount++;
                Connection.Update(f);
            }
        }

        public void Dereference(IEnumerable<FileInfo> files)
        {
            foreach (var f in files)
            {
                f.ReferenceCount--;
                Connection.Update(f);
            }
        }

        private void deletePending()
        {
            foreach (var f in QueryAndPopulate<FileInfo>(f => f.ReferenceCount < 1))
            {
                try
                {
                    Connection.Delete(f);
                    Storage.Delete(Path.Combine(prefix, f.Hash));
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not delete beatmap {f}");
                }
            }
        }
    }
}