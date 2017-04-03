// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Linq;
using Ionic.Zip;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Beatmaps.IO
{
    public sealed class OszArchiveReader : ArchiveReader
    {
        public static void Register()
        {
            AddReader<OszArchiveReader>((storage, path) =>
            {
                using (var stream = storage.GetStream(path))
                    return ZipFile.IsZipFile(stream, false);
            });
            OsuLegacyDecoder.Register();
        }

        private readonly Stream archiveStream;
        private readonly ZipFile archive;

        public OszArchiveReader(Stream archiveStream)
        {
            this.archiveStream = archiveStream;
            archive = ZipFile.Read(archiveStream);

            BeatmapFilenames = archive.Entries.Where(e => e.FileName.EndsWith(@".osu")).Select(e => e.FileName).ToArray();

            if (BeatmapFilenames.Length == 0)
                throw new FileNotFoundException(@"This directory contains no beatmaps");

            StoryboardFilename = archive.Entries.Where(e => e.FileName.EndsWith(@".osb")).Select(e => e.FileName).FirstOrDefault();
        }

        public override Stream GetStream(string name)
        {
            ZipEntry entry = archive.Entries.SingleOrDefault(e => e.FileName == name);
            if (entry == null)
                throw new FileNotFoundException();
            return entry.OpenReader();
        }

        public override void Dispose()
        {
            archive.Dispose();
            archiveStream.Dispose();
        }
    }
}