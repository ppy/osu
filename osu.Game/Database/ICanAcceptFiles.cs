// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which can accept files for importing.
    /// </summary>
    public interface ICanAcceptFiles
    {
        /// <summary>
        /// Import the specified paths.
        /// </summary>
        /// <param name="paths">The files which should be imported.</param>
        Task Import(params string[] paths);

        /// <summary>
        /// Import the provided stream as a simple item.
        /// </summary>
        /// <param name="stream">The stream to import files from. Should be in a supported archive format.</param>
        /// <param name="filename">The filename of the archive being imported.</param>
        Task Import(Stream stream, string filename);

        /// <summary>
        /// An array of accepted file extensions (in the standard format of ".abc").
        /// </summary>
        IEnumerable<string> HandledExtensions { get; }
    }
}
