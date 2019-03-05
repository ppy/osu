// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Game.IO.Archives
{
    /// <summary>
    /// Reads an archive from a directory on disk.
    /// </summary>
    public class LegacyDirectoryArchiveReader : ArchiveReader
    {
        private readonly string path;

        public LegacyDirectoryArchiveReader(string path)
            : base(Path.GetFileName(path))
        {
            // re-get full path to standardise with Directory.GetFiles return values below.
            this.path = Path.GetFullPath(path);
        }

        public override Stream GetStream(string name) => File.OpenRead(Path.Combine(path, name));

        public override void Dispose()
        {
        }

        public override IEnumerable<string> Filenames => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(f => f.Replace(path, string.Empty).Trim(Path.DirectorySeparatorChar)).ToArray();

        public override Stream GetUnderlyingStream() => null;
    }
}
