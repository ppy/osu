// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Game.IO.Archives
{
    /// <summary>
    /// Reads an archive directly from a directory on disk.
    /// </summary>
    public class DirectoryArchiveReader : ArchiveReader
    {
        private readonly string path;

        public DirectoryArchiveReader(string path)
            : base(Path.GetFileName(path))
        {
            // re-get full path to standardise with Directory.GetFiles return values below.
            this.path = Path.GetFullPath(path);
        }

        public override Stream GetStream(string name) => File.OpenRead(GetFullPath(name));

        public string GetFullPath(string filename) => Path.Combine(path, filename);

        public override void Dispose()
        {
        }

        public override IEnumerable<string> Filenames => Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(f => f.Replace(path, string.Empty).Trim(Path.DirectorySeparatorChar)).ToArray();
    }
}
