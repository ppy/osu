// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Database;

namespace osu.Game
{
    public partial class OsuGameBase
    {
        private readonly List<ICanAcceptFiles> fileImporters = new List<ICanAcceptFiles>();

        /// <summary>
        /// Register a global handler for file imports. Most recently registered will have precedence.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        public void RegisterImportHandler(ICanAcceptFiles handler) => fileImporters.Insert(0, handler);

        /// <summary>
        /// Unregister a global handler for file imports.
        /// </summary>
        /// <param name="handler">The previously registered handler.</param>
        public void UnregisterImportHandler(ICanAcceptFiles handler) => fileImporters.Remove(handler);

        public async Task Import(params string[] paths)
        {
            if (paths.Length == 0)
                return;

            var filesPerExtension = paths.GroupBy(p => Path.GetExtension(p).ToLowerInvariant());

            foreach (var groups in filesPerExtension)
            {
                foreach (var importer in fileImporters)
                {
                    if (importer.HandledExtensions.Contains(groups.Key))
                        await importer.Import(groups.ToArray()).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task Import(ImportTask[] tasks, ImportParameters parameters = default)
        {
            var tasksPerExtension = tasks.GroupBy(t => Path.GetExtension(t.Path).ToLowerInvariant());
            await Task.WhenAll(tasksPerExtension.Select(taskGroup =>
            {
                var importer = fileImporters.FirstOrDefault(i => i.HandledExtensions.Contains(taskGroup.Key));
                return importer?.Import(taskGroup.ToArray(), parameters) ?? Task.CompletedTask;
            })).ConfigureAwait(false);
        }

        public IEnumerable<string> HandledExtensions => fileImporters.SelectMany(i => i.HandledExtensions);
    }
}
