// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Beatmaps.IO
{
    public abstract class BeatmapArchiveReader : ArchiveReader
    {
        public static BeatmapArchiveReader GetBeatmapArchiveReader(Storage storage, string path)
        {
            Func<ArchiveReader.Reader, bool> testBeatmapArchiveReader = (ArchiveReader.Reader r) => {
                return typeof(BeatmapArchiveReader).IsAssignableFrom(r.Type);
            };
            return (BeatmapArchiveReader)GetReader(storage, path, testBeatmapArchiveReader);
        }

        /// <summary>
        /// Reads the beatmap metadata from this archive.
        /// </summary>
        public abstract BeatmapMetadata ReadMetadata();

        /// <summary>
        /// Gets a list of beatmap file names.
        /// </summary>
        public string[] BeatmapFilenames { get; protected set; }

        /// <summary>
        /// The storyboard filename. Null if no storyboard is present.
        /// </summary>
        public string StoryboardFilename { get; protected set; }
    }
}