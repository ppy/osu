// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;

namespace osu.Game.IO.Archives
{
    /// <summary>
    /// Allows reading a single file from the provided byte array.
    /// </summary>
    public class ByteArrayArchiveReader : ArchiveReader
    {
        private readonly byte[] content;

        public ByteArrayArchiveReader(byte[] content, string filename)
            : base(filename)
        {
            this.content = content;
        }

        public override Stream GetStream(string name) => new MemoryStream(content);

        public override void Dispose()
        {
        }

        public override IEnumerable<string> Filenames => new[] { Name };
    }
}
