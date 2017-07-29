﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace osu.Game.Beatmaps.IO
{
    public sealed class OszArchiveReader : ArchiveReader
    {
        private readonly Stream archiveStream;
        private readonly ZipFile archive;

        public OszArchiveReader(Stream archiveStream)
        {
            this.archiveStream = archiveStream;
            archive = ZipFile.Read(archiveStream);
        }

        public override Stream GetStream(string name)
        {
            ZipEntry entry = archive.Entries.SingleOrDefault(e => e.FileName == name);
            if (entry == null)
                throw new FileNotFoundException();

            // allow seeking
            MemoryStream copy = new MemoryStream();

            using (Stream s = entry.OpenReader())
                s.CopyTo(copy);

            copy.Position = 0;

            return copy;
        }

        public override void Dispose()
        {
            archive.Dispose();
            archiveStream.Dispose();
        }

        public override IEnumerable<string> Filenames => archive.Entries.Select(e => e.FileName).ToArray();

        public override Stream GetUnderlyingStream() => archiveStream;
    }
}