// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Game.Beatmaps.IO
{
    /// <summary>
    /// Reads an extracted legacy beatmap from disk.
    /// </summary>
    public class LegacyFilesystemReader : ArchiveReader
    {
        private readonly string path;

        public LegacyFilesystemReader(string path)
        {
            this.path = path;
        }

        public override Stream GetStream(string name) => File.OpenRead(Path.Combine(path, name));

        public override void Dispose()
        {
            // no-op
        }

        public override IEnumerable<string> Filenames => Directory.GetFiles(path).Select(Path.GetFileName).ToArray();

        public override Stream GetUnderlyingStream() => null;
    }
}
