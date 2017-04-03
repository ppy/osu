// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Linq;
using osu.Game.Beatmaps.IO;

namespace osu.Desktop.Beatmaps.IO
{
    /// <summary>
    /// Reads an extracted legacy beatmap from disk.
    /// </summary>
    public class LegacyFilesystemReader : ArchiveReader
    {
        public static void Register() => AddReader<LegacyFilesystemReader>((storage, path) => Directory.Exists(path));

        private string basePath { get; }

        public LegacyFilesystemReader(string path)
        {
            basePath = path;

            BeatmapFilenames = Directory.GetFiles(basePath, @"*.osu").Select(Path.GetFileName).ToArray();

            if (BeatmapFilenames.Length == 0)
                throw new FileNotFoundException(@"This directory contains no beatmaps");

            StoryboardFilename = Directory.GetFiles(basePath, @"*.osb").Select(Path.GetFileName).FirstOrDefault();
        }

        public override Stream GetStream(string name)
        {
            return File.OpenRead(Path.Combine(basePath, name));
        }

        public override void Dispose()
        {
            // no-op
        }
    }
}
