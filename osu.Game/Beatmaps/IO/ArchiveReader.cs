﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Beatmaps.IO
{
    public abstract class ArchiveReader : IDisposable, IResourceStore<byte[]>
    {
        protected enum ArchiveStorageMethod
        {
            Directory, File
        }

        private class Reader
        {
            public Func<BasicStorage, string, bool> Test { get; set; }
            public Type Type { get; set; }
            public ArchiveStorageMethod ArchiveStorageMethod { get; set; }
        }

        private static List<Reader> readers { get; } = new List<Reader>();

        public static ArchiveReader GetReader(BasicStorage storage, string path)
        {
            foreach (var reader in readers)
            {
                if (!reader.Test(storage, path)) continue;

                switch (reader.ArchiveStorageMethod)
                {
                    case ArchiveStorageMethod.Directory:
                        return (ArchiveReader)Activator.CreateInstance(reader.Type, path);
                    default:
                        return (ArchiveReader)Activator.CreateInstance(reader.Type, storage.GetStream(path));
                }
            }
            throw new IOException(@"Unknown file format");
        }
        
        protected static void AddReader<T>(Func<BasicStorage, string, bool> test, ArchiveStorageMethod archiveStorageMethod) where T : ArchiveReader
        {
            readers.Add(new Reader { Test = test, Type = typeof(T), ArchiveStorageMethod = archiveStorageMethod});
        }
    
        /// <summary>
        /// Reads the beatmap metadata from this archive.
        /// </summary>
        public abstract BeatmapMetadata ReadMetadata();
        /// <summary>
        /// Gets a list of beatmap file names.
        /// </summary>
        public abstract string[] ReadBeatmaps();
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