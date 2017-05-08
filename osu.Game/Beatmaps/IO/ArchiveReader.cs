// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Beatmaps.IO
{
    public abstract class ArchiveReader : IDisposable, IResourceStore<byte[]>
    {
        private class Reader
        {
            public Func<Storage, string, bool> Test;
            public Type Type;
        }

        private static readonly List<Reader> readers = new List<Reader>();

        public static ArchiveReader GetReader(Storage storage, string path)
        {
            foreach (var reader in readers)
            {
                if (reader.Test(storage, path))
                    return (ArchiveReader)Activator.CreateInstance(reader.Type, storage.GetStream(path));
            }
            throw new IOException(@"Unknown file format");
        }

        protected static void AddReader<T>(Func<Storage, string, bool> test) where T : ArchiveReader
        {
            readers.Add(new Reader { Test = test, Type = typeof(T) });
        }

        /// <summary>
        /// Gets a list of beatmap file names.
        /// </summary>
        public string[] BeatmapFilenames { get; protected set; }

        /// <summary>
        /// The storyboard filename. Null if no storyboard is present.
        /// </summary>
        public string StoryboardFilename { get; protected set; }

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

                byte[] buffer = new byte[input.Length];
                input.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}