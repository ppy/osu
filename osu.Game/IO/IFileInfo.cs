// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.IO
{
    /// <summary>
    /// A representation of a tracked file.
    /// </summary>
    public interface IFileInfo
    {
        /// <summary>
        /// SHA-256 hash of the file content.
        /// </summary>
        string Hash { get; }
    }
}
