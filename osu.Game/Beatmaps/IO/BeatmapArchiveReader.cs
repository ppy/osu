// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Beatmaps.IO
{
    public abstract class BeatmapArchiveReader : ArchiveReader
    {

        public static BeatmapArchiveReader GetBeatmapArchiveReader(Storage storage, string path)
        {
            foreach (var reader in Readers)
            {
                if (reader.Test(storage, path))
                    return (BeatmapArchiveReader)Activator.CreateInstance(reader.Type, storage.GetStream(path));
            }
            throw new IOException(@"Unknown file format");
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