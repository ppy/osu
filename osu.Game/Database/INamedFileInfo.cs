// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.IO;

namespace osu.Game.Database
{
    /// <summary>
    /// Represent a join model which gives a filename and scope to a <see cref="FileInfo"/>.
    /// </summary>
    public interface INamedFileInfo
    {
        // An explicit foreign key property isn't required but is recommended and may be helpful to have
        int FileInfoID { get; set; }

        FileInfo FileInfo { get; set; }

        string Filename { get; set; }
    }
}
