// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which can accept files for importing.
    /// </summary>
    public interface ICanAcceptFiles
    {
        /// <summary>
        /// Import one or more items from filesystem <paramref name="paths"/>.
        /// </summary>
        /// <remarks>
        /// This will be treated as a low priority batch import if more than one path is specified.
        /// This will post notifications tracking progress.
        /// </remarks>
        /// <param name="paths">The files which should be imported.</param>
        Task Import(params string[] paths);

        /// <summary>
        /// Import the specified files from the given import tasks.
        /// </summary>
        /// <remarks>
        /// This will be treated as a low priority batch import if more than one path is specified.
        /// This will post notifications tracking progress.
        /// </remarks>
        /// <param name="tasks">The import tasks from which the files should be imported.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        Task Import(ImportTask[] tasks, ImportParameters parameters = default);

        /// <summary>
        /// An array of accepted file extensions (in the standard format of ".abc").
        /// </summary>
        IEnumerable<string> HandledExtensions { get; }
    }
}
