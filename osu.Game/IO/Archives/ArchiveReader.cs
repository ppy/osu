// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.IO.Stores;

namespace osu.Game.IO.Archives
{
    public abstract class ArchiveReader : IResourceStore<byte[]>
    {
        /// <summary>
        /// Opens a stream for reading a specific file from this archive.
        /// </summary>
        public abstract Stream GetStream(string name);

        public IEnumerable<string> GetAvailableResources() => Filenames;

        public abstract void Dispose();

        /// <summary>
        /// The name of this archive (usually the containing filename).
        /// </summary>
        public readonly string Name;

        protected ArchiveReader(string name)
        {
            Name = name;
        }

        public abstract IEnumerable<string> Filenames { get; }

        public virtual byte[] Get(string name) => GetAsync(name).Result;

        public async Task<byte[]> GetAsync(string name)
        {
            using (Stream input = GetStream(name))
            {
                if (input == null)
                    return null;

                byte[] buffer = new byte[input.Length];
                await input.ReadAsync(buffer);
                return buffer;
            }
        }
    }
}
