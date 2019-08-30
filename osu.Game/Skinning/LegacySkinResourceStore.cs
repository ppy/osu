// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.IO.Stores;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class LegacySkinResourceStore<T> : IResourceStore<byte[]>
        where T : INamedFileInfo
    {
        private readonly IHasFiles<T> source;
        private readonly IResourceStore<byte[]> underlyingStore;

        private string getPathForFile(string filename)
        {
            if (source.Files == null)
                return null;

            bool hasExtension = filename.Contains('.');

            var file = source.Files.Find(f =>
                string.Equals(hasExtension ? f.Filename : Path.ChangeExtension(f.Filename, null), filename, StringComparison.InvariantCultureIgnoreCase));
            return file?.FileInfo.StoragePath;
        }

        public LegacySkinResourceStore(IHasFiles<T> source, IResourceStore<byte[]> underlyingStore)
        {
            this.source = source;
            this.underlyingStore = underlyingStore;
        }

        public Stream GetStream(string name)
        {
            string path = getPathForFile(name);
            return path == null ? null : underlyingStore.GetStream(path);
        }

        public IEnumerable<string> GetAvailableResources() => source.Files.Select(f => f.Filename);

        byte[] IResourceStore<byte[]>.Get(string name) => GetAsync(name).Result;

        public Task<byte[]> GetAsync(string name)
        {
            string path = getPathForFile(name);
            return path == null ? Task.FromResult<byte[]>(null) : underlyingStore.GetAsync(path);
        }

        #region IDisposable Support

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
            }
        }

        ~LegacySkinResourceStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
