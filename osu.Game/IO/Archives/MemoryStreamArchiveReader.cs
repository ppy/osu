// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;

namespace osu.Game.IO.Archives
{
    /// <summary>
    /// Allows reading a single file from the provided memory stream.
    /// </summary>
    public class MemoryStreamArchiveReader : ArchiveReader
    {
        private readonly MemoryStream stream;

        public MemoryStreamArchiveReader(MemoryStream stream, string filename)
            : base(filename)
        {
            this.stream = stream;
        }

        public override Stream GetStream(string name) => new MemoryStream(stream.ToArray(), 0, (int)stream.Length);

        public override void Dispose()
        {
        }

        public override IEnumerable<string> Filenames => new[] { Name };
    }
}
