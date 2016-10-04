using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.Beatmaps;

namespace osu.Desktop.Beatmaps.IO
{
    /// <summary>
    /// Reads an extracted legacy beatmap from disk.
    /// </summary>
    public class LegacyFilesystemReader : ArchiveReader
    {
        static LegacyFilesystemReader()
        {
            AddReader<LegacyFilesystemReader>((storage, path) => Directory.Exists(path));
        }
    
        private string BasePath { get; set; }
        private Beatmap FirstMap { get; set; }
    
        public LegacyFilesystemReader(string path)
        {
            BasePath = path;
            var maps = ReadBeatmaps();
            if (maps.Length == 0)
                throw new FileNotFoundException("This directory contains no beatmaps");
            using (var stream = new StreamReader(ReadFile(maps[0])))
            {
                var decoder = BeatmapDecoder.GetDecoder(stream);
                FirstMap = decoder.Decode(stream);
            }
        }

        public override string[] ReadBeatmaps()
        {
            return Directory.GetFiles(BasePath, "*.osu").Select(f => Path.GetFileName(f)).ToArray();
        }

        public override Stream ReadFile(string name)
        {
            return File.OpenRead(Path.Combine(BasePath, name));
        }

        public override Metadata ReadMetadata()
        {
            return FirstMap.Metadata;
        }    }
}