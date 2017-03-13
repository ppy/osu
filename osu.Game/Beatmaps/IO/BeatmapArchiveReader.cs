// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Beatmaps.IO
{
    public abstract class BeatmapArchiveReader : ArchiveReader
    {
        public const string OszExtension = @".osz";

        public static BeatmapArchiveReader GetBeatmapArchiveReader(Storage storage, string path)
        {
            try
            {
                return (BeatmapArchiveReader)GetReader(storage, path);
            }
            catch (InvalidCastException e)
            {
                Logger.Error(e, "A tricky  " + $@"{nameof(ArchiveReader)}" + " instance passed the test to be a " + $@"{nameof(BeatmapArchiveReader)}" + ", but it's really not");
                throw;
            }
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