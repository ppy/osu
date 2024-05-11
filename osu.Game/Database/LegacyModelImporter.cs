// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles importing legacy user data of a single type from osu-stable.
    /// </summary>
    public abstract class LegacyModelImporter<TModel>
        where TModel : class, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The relative path from osu-stable's data directory to import items from.
        /// </summary>
        protected virtual string ImportFromStablePath => null;

        /// <summary>
        /// Select paths to import from stable where all paths should be absolute. Default implementation iterates all directories in <see cref="ImportFromStablePath"/>.
        /// </summary>
        protected virtual IEnumerable<string> GetStableImportPaths(Storage storage)
        {
            if (!storage.ExistsDirectory(ImportFromStablePath))
                return Enumerable.Empty<string>();

            return storage.GetDirectories(ImportFromStablePath)
                          .Select(path => storage.GetFullPath(path));
        }

        protected readonly IModelImporter<TModel> Importer;

        protected LegacyModelImporter(IModelImporter<TModel> importer)
        {
            Importer = importer;
        }

        public Task<int> GetAvailableCount(StableStorage stableStorage) => Task.Run(() => GetStableImportPaths(PrepareStableStorage(stableStorage)).Count());

        public Task ImportFromStableAsync(StableStorage stableStorage)
        {
            var storage = PrepareStableStorage(stableStorage);

            // Handle situations like when the user does not have a Skins folder.
            if (!storage.ExistsDirectory(ImportFromStablePath))
            {
                string fullPath = storage.GetFullPath(ImportFromStablePath);

                Logger.Log(@$"Folder ""{fullPath}"" not available in the target osu!stable installation to import {Importer.HumanisedModelName}s.", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            return Task.Run(async () =>
            {
                var tasks = GetStableImportPaths(storage).Select(p => new ImportTask(p)).ToArray();

                await Importer.Import(tasks, new ImportParameters { Batch = true, PreferHardLinks = true }).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Run any required traversal operations on the stable storage location before performing operations.
        /// </summary>
        /// <param name="stableStorage">The stable storage.</param>
        /// <returns>The usable storage. Return the unchanged <paramref name="stableStorage"/> if no traversal is required.</returns>
        protected virtual Storage PrepareStableStorage(StableStorage stableStorage) => stableStorage;
    }
}
