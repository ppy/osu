using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.OS;

namespace osu.Game.Beatmaps.IO
{
    public abstract class ArchiveReader
    {
        private class Reader
        {
            public Func<BasicStorage, string, bool> Test { get; set; }
            public Type Type { get; set; }
        }
    
        private static List<Reader> Readers { get; set; } = new List<Reader>();
    
        public static ArchiveReader GetReader(BasicStorage storage, string path)
        {
            foreach (var reader in Readers)
            {
                if (reader.Test(storage, path))
                    return (ArchiveReader)Activator.CreateInstance(reader.Type);
            }
            throw new IOException("Unknown file format");
        }
        
        protected static void AddReader<T>(Func<BasicStorage, string, bool> test) where T : ArchiveReader
        {
            Readers.Add(new Reader { Test = test, Type = typeof(T) });
        }
    
        /// <summary>
        /// Reads the beatmap metadata from this archive.
        /// </summary>
        public abstract Metadata ReadMetadata();
        /// <summary>
        /// Gets a list of beatmap file names.
        /// </summary>
        public abstract string[] ReadBeatmaps();
        /// <summary>
        /// Opens a stream for reading a specific file from this archive.
        /// </summary>
        public abstract Stream ReadFile(string name);
    }
}