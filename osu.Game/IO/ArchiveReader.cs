// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.IO
{
    public abstract class ArchiveReader : IDisposable, IResourceStore<byte[]>
    {
        protected class Reader
        {
            public Func<Storage, string, bool> Test { get; set; }
            public Type Type { get; set; }
        }

        protected static List<Reader> Readers { get; } = new List<Reader>();

        public static ArchiveReader GetReader(Storage storage, string path)
        {
            foreach (var reader in Readers)
            {
                if (reader.Test(storage, path))
                    return (ArchiveReader)Activator.CreateInstance(reader.Type, storage.GetStream(path));
            }
            throw new IOException(@"Unknown file format");
        }

        protected static void AddReader<T>(Func<Storage, string, bool> test) where T : ArchiveReader
        {
            Readers.Add(new Reader { Test = test, Type = typeof(T) });
        }

        /// <summary>
        /// Opens a stream for reading a specific file from this archive.
        /// </summary>
        public abstract Stream GetStream(string name);

        public abstract void Dispose();

        public virtual byte[] Get(string name)
        {
            using (Stream input = GetStream(name))
            {
                if (input == null)
                    return null;

                using (MemoryStream ms = new MemoryStream())
                {
                    input.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}