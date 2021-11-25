// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Extensions;

namespace osu.Game.IO
{
    /// <summary>
    /// Handles the Store and retrieval of Files/FileSets to the database backing
    /// </summary>
    public class FileStore : DatabaseBackedStore
    {
        public readonly IResourceStore<byte[]> Store;

        public new Storage Storage => base.Storage;

        public FileStore(IDatabaseContextFactory contextFactory, Storage storage)
            : base(contextFactory, storage.GetStorageForDirectory(@"files"))
        {
            Store = new StorageBackedResourceStore(Storage);
        }

        public FileInfo Add(Stream data, bool reference = true)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                string hash = data.ComputeSHA2Hash();

                var existing = usage.Context.FileInfo.FirstOrDefault(f => f.Hash == hash);

                var info = existing ?? new FileInfo { Hash = hash };

                string path = info.GetStoragePath();

                // we may be re-adding a file to fix missing store entries.
                bool requiresCopy = !Storage.Exists(path);

                if (!requiresCopy)
                {
                    // even if the file already exists, check the existing checksum for safety.
                    using (var stream = Storage.GetStream(path))
                        requiresCopy |= stream.ComputeSHA2Hash() != hash;
                }

                if (requiresCopy)
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
        }

        public void Reference(params FileInfo[] files)
        {
            if (files.Length == 0) return;

            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                foreach (var f in files.GroupBy(f => f.ID))
                {
                    var refetch = context.Find<FileInfo>(f.First().ID) ?? f.First();
                    refetch.ReferenceCount += f.Count();
                    context.FileInfo.Update(refetch);
                }
            }
        }

        public void Dereference(params FileInfo[] files)
        {
            if (files.Length == 0) return;

            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                foreach (var f in files.GroupBy(f => f.ID))
                {
                    var refetch = context.FileInfo.Find(f.Key);
                    refetch.ReferenceCount -= f.Count();
                    context.FileInfo.Update(refetch);
                }
            }
        }

        public override void Cleanup()
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                foreach (var f in context.FileInfo.Where(f => f.ReferenceCount < 1))
                {
                    try
                    {
                        Storage.Delete(f.GetStoragePath());
                        context.FileInfo.Remove(f);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete beatmap {f}");
                    }
                }
            }
        }
    }
}
