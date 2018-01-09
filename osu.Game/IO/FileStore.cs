// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.IO
{
    /// <summary>
    /// Handles the Store and retrieval of Files/FileSets to the database backing
    /// </summary>
    public class FileStore : DatabaseBackedStore
    {
        public readonly IResourceStore<byte[]> Store;

        public new Storage Storage => base.Storage;

        public FileStore(Func<OsuDbContext> createContext, Storage storage) : base(createContext, storage.GetStorageForDirectory(@"files"))
        {
            Store = new StorageBackedResourceStore(Storage);
        }

        public FileInfo Add(Stream data, bool reference = true)
        {
            var context = GetContext();

            string hash = data.ComputeSHA2Hash();

            var existing = context.FileInfo.FirstOrDefault(f => f.Hash == hash);

            var info = existing ?? new FileInfo { Hash = hash };

            string path = info.StoragePath;

            // we may be re-adding a file to fix missing store entries.
            if (!Storage.Exists(path))
            {
                data.Seek(0, SeekOrigin.Begin);

                using (var output = Storage.GetStream(path, FileAccess.Write))
                    data.CopyTo(output);

                data.Seek(0, SeekOrigin.Begin);
            }

            if (reference || existing == null)
                Reference(info);

            return info;
        }

        public void Reference(params FileInfo[] files) => reference(GetContext(), files);

        private void reference(OsuDbContext context, FileInfo[] files)
        {
            foreach (var f in files.GroupBy(f => f.ID))
            {
                var refetch = context.Find<FileInfo>(f.First().ID) ?? f.First();
                refetch.ReferenceCount += f.Count();
                context.FileInfo.Update(refetch);
            }

            context.SaveChanges();
        }

        public void Dereference(params FileInfo[] files) => dereference(GetContext(), files);

        private void dereference(OsuDbContext context, FileInfo[] files)
        {
            foreach (var f in files.GroupBy(f => f.ID))
            {
                var refetch = context.FileInfo.Find(f.Key);
                refetch.ReferenceCount -= f.Count();
                context.FileInfo.Update(refetch);
            }

            context.SaveChanges();
        }

        public override void Cleanup()
        {
            var context = GetContext();

            foreach (var f in context.FileInfo.Where(f => f.ReferenceCount < 1))
            {
                try
                {
                    Storage.Delete(f.StoragePath);
                    context.FileInfo.Remove(f);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not delete beatmap {f}");
                }
            }

            context.SaveChanges();
        }
    }
}
