// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Linq;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Desktop.Beatmaps.IO
{
    /// <summary>
    /// Reads an extracted legacy beatmap from disk.
    /// </summary>
    public class LegacyFilesystemReader : ArchiveReader
    {
        public static void Register() => AddReader<LegacyFilesystemReader>((storage, path) => Directory.Exists(path));

        private string basePath { get; set; }
        private Beatmap firstMap { get; set; }

        public LegacyFilesystemReader(string path)
        {
            basePath = path;
            BeatmapFilenames = Directory.GetFiles(basePath, @"*.osu").Select(f => Path.GetFileName(f)).ToArray();
            if (BeatmapFilenames.Length == 0)
                throw new FileNotFoundException(@"This directory contains no beatmaps");
            StoryboardFilename = Directory.GetFiles(basePath, @"*.osb").Select(f => Path.GetFileName(f)).FirstOrDefault();
            using (var stream = new StreamReader(GetStream(BeatmapFilenames[0])))
            {
                var decoder = BeatmapDecoder.GetDecoder(stream);
                firstMap = decoder.Decode(stream);
            }
        }

        public override Stream GetStream(string name)
        {
            return File.OpenRead(Path.Combine(basePath, name));
        }

        public override BeatmapMetadata ReadMetadata()
        {
            return firstMap.BeatmapInfo.Metadata;
        }

        public override void Dispose()
        {
            // no-op
        }
    }
}
