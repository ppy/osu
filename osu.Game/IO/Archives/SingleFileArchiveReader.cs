// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;

namespace osu.Game.IO.Archives
{
    /// <summary>
    /// Reads a single file on disk as an archive.
    /// Note: In this case, the file is not an extractable archive, use <see cref="ZipArchiveReader"/> instead.
    /// </summary>
    public class SingleFileArchiveReader : ArchiveReader
    {
        private readonly string path;

        public SingleFileArchiveReader(string path)
            : base(Path.GetFileName(path))
        {
            // re-get full path to standardise
            this.path = Path.GetFullPath(path);
        }

        public override Stream GetStream(string name) => File.OpenRead(path);

        public override void Dispose()
        {
        }

        public override IEnumerable<string> Filenames => new[] { Name };
    }
}
