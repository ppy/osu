// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;

namespace osu.Game.IO
{
    public class FileInfo : IHasPrimaryKey, IFileInfo
    {
        public int ID { get; set; }

        public string Hash { get; set; }

        public int ReferenceCount { get; set; }
    }
}
