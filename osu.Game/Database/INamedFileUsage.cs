// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.IO;

namespace osu.Game.Database
{
    /// <summary>
    /// A usage of a file, with a local filename attached.
    /// </summary>
    public interface INamedFileUsage
    {
        /// <summary>
        /// The underlying file on disk.
        /// </summary>
        IFileInfo File { get; }

        /// <summary>
        /// The filename for this usage.
        /// </summary>
        string Filename { get; }
    }
}
